import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/query-client';
import { GiftSettingsPage } from './GiftSettingsPage';

afterEach(() => vi.unstubAllGlobals());

const ring = {
  id: 'd1',
  version: 5,
  recipient: 'Maria',
  label: 'Кольцо',
  value: '18,5–19,0',
  sortOrder: 1,
};
const shoes = {
  id: 'd2',
  version: 2,
  recipient: 'Vitaliy',
  label: 'Обувь',
  value: '43',
  sortOrder: 1,
};
const books = { id: 'c1', name: 'Книги', sortOrder: 10 };

const json = (body: unknown, status = 200) =>
  Promise.resolve(
    new Response(body === undefined ? null : JSON.stringify(body), {
      status,
      headers: { 'Content-Type': 'application/json' },
    }),
  );

interface Sent {
  method: string;
  url: URL;
  headers: Headers;
  body: unknown;
}

function renderSettings(
  data: { details: unknown[]; categories: unknown[] },
  answer?: (sent: Sent) => Promise<Response> | undefined,
) {
  const sent: Sent[] = [];
  vi.stubGlobal(
    'fetch',
    vi.fn(async (input: Request) => {
      const text = await input.text();
      const entry = {
        method: input.method,
        url: new URL(input.url),
        headers: input.headers,
        body: text ? JSON.parse(text) : undefined,
      };
      sent.push(entry);
      const custom = answer?.(entry);
      if (custom) return custom;
      if (entry.method === 'GET')
        return json(
          entry.url.pathname.endsWith('/details')
            ? data.details
            : data.categories,
        );
      return json(
        entry.method === 'POST' ? { id: 'new' } : undefined,
        entry.method === 'POST' ? 201 : 204,
      );
    }),
  );
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter>
          <GiftSettingsPage />
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  return { writes: () => sent.filter((s) => s.method !== 'GET'), sent };
}

test('shows the нюанси grouped by person', async () => {
  renderSettings({ details: [ring, shoes], categories: [books] });
  const card = await screen.findByRole('region', { name: 'Нюанси' });
  const masha = await within(card).findByRole('list', { name: 'Маша' });
  expect(within(masha).getByText('Кольцо')).toBeInTheDocument();
  expect(within(masha).getByText('18,5–19,0')).toBeInTheDocument();
  expect(within(card).getByRole('list', { name: 'Віталій' })).toHaveTextContent(
    'Обувь',
  );
  expect(within(card).queryByRole('list', { name: 'Разом' })).toBeNull();
});

test('says when there are no нюанси yet', async () => {
  renderSettings({ details: [], categories: [] });
  expect(await screen.findByText(/Нюансів поки немає/)).toBeInTheDocument();
  expect(await screen.findByText(/Категорій поки немає/)).toBeInTheDocument();
});

test('adds a нюанс for a person with one key, and edits one with its version', async () => {
  const { writes } = renderSettings({ details: [ring], categories: [books] });
  await userEvent.click(
    await screen.findByRole('button', { name: 'Додати нюанс для Маша' }),
  );
  const dialog = screen.getByRole('dialog', { name: 'Новий нюанс' });
  expect(
    within(dialog).getByRole('combobox', { name: 'Для кого' }),
  ).toHaveTextContent('Маша');
  await userEvent.type(within(dialog).getByLabelText('Що'), 'Браслет');
  await userEvent.type(within(dialog).getByLabelText('Значення'), '17 см');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  await waitFor(() => expect(screen.queryByRole('dialog')).toBeNull());
  const post = writes()[0]!;
  expect(post.url.pathname).toBe('/api/gifts/details');
  expect(post.headers.get('Idempotency-Key')).toBeTruthy();
  expect(post.body).toEqual({
    recipient: 'Maria',
    label: 'Браслет',
    value: '17 см',
  });

  await userEvent.click(screen.getByRole('button', { name: 'Дії: Кольцо' }));
  await userEvent.click(screen.getByRole('menuitem', { name: 'Змінити' }));
  const edit = screen.getByRole('dialog', { name: 'Змінити нюанс' });
  await userEvent.clear(within(edit).getByLabelText('Значення'));
  await userEvent.type(within(edit).getByLabelText('Значення'), '19');
  await userEvent.click(within(edit).getByRole('button', { name: 'Зберегти' }));
  await waitFor(() => expect(writes()).toHaveLength(2));
  expect(writes()[1]!.url.pathname).toBe('/api/gifts/details/d1');
  expect(writes()[1]!.headers.get('If-Match')).toBe('5');
});

test('deletes a нюанс after asking', async () => {
  const { writes } = renderSettings({ details: [ring], categories: [] });
  await userEvent.click(
    await screen.findByRole('button', { name: 'Дії: Кольцо' }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Видалити' }));
  await userEvent.click(
    within(screen.getByRole('dialog', { name: 'Видалити нюанс?' })).getByRole(
      'button',
      { name: 'Видалити' },
    ),
  );
  await waitFor(() => expect(writes()[0]).toMatchObject({ method: 'DELETE' }));
  expect(writes()[0]!.url.pathname).toBe('/api/gifts/details/d1');
});

test('adds, renames and refuses to delete a used category', async () => {
  const { writes } = renderSettings(
    { details: [], categories: [books] },
    (sent) => {
      if (sent.method === 'DELETE')
        return json({ detail: 'x', code: 'gifts.category_in_use' }, 409);
      if (sent.method === 'POST')
        return json({ detail: 'x', code: 'gifts.category_name_taken' }, 409);
      return undefined;
    },
  );
  await userEvent.click(
    await screen.findByRole('button', { name: 'Додати категорію' }),
  );
  const add = screen.getByRole('dialog', { name: 'Нова категорія' });
  await userEvent.type(within(add).getByLabelText('Назва'), 'Книги');
  await userEvent.click(within(add).getByRole('button', { name: 'Зберегти' }));
  expect(
    await within(add).findByText('Така категорія вже є'),
  ).toBeInTheDocument();
  await userEvent.click(within(add).getByRole('button', { name: 'Скасувати' }));

  await userEvent.click(screen.getByRole('button', { name: 'Дії: Книги' }));
  await userEvent.click(screen.getByRole('menuitem', { name: 'Змінити' }));
  const rename = screen.getByRole('dialog', {
    name: 'Перейменувати категорію',
  });
  await userEvent.type(within(rename).getByLabelText('Назва'), ' и комиксы');
  await userEvent.click(
    within(rename).getByRole('button', { name: 'Зберегти' }),
  );
  await waitFor(() => expect(screen.queryByRole('dialog')).toBeNull());
  const put = writes().find((w) => w.method === 'PUT')!;
  expect(put.url.pathname).toBe('/api/gifts/categories/c1');
  expect(put.headers.get('If-Match')).toBeNull();
  expect(put.body).toEqual({ name: 'Книги и комиксы' });

  await userEvent.click(screen.getByRole('button', { name: 'Дії: Книги' }));
  await userEvent.click(screen.getByRole('menuitem', { name: 'Видалити' }));
  const remove = screen.getByRole('dialog', { name: 'Видалити категорію?' });
  await userEvent.click(
    within(remove).getByRole('button', { name: 'Видалити' }),
  );
  expect(
    await within(remove).findByText(
      'Ця категорія є в подарунків — спершу перенесіть їх в іншу.',
    ),
  ).toBeInTheDocument();
});
