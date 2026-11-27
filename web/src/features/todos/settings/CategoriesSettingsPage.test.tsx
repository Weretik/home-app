import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/client/query-client';
import { TodosSection } from '../section/TodosSection';
import { CategoriesSettingsPage } from './CategoriesSettingsPage';

afterEach(() => vi.unstubAllGlobals());

const categories = [
  {
    id: 'c-home',
    name: 'Дом',
    emoji: '🏠',
    sortOrder: 10,
    version: 1,
    activeCount: 2,
  },
];

const json = (body: unknown, status = 200) =>
  Promise.resolve(
    new Response(JSON.stringify(body), {
      status,
      headers: { 'Content-Type': 'application/json' },
    }),
  );
const noContent = () => Promise.resolve(new Response(null, { status: 204 }));

interface Sent {
  method: string;
  url: URL;
  headers: Headers;
  body: unknown;
}

function LocationProbe() {
  return <output data-testid="location">{useLocation().search}</output>;
}

/** Stubs the API: categories always answer; `answer` handles the rest. */
function renderPage(
  answer: (sent: Sent) => Promise<Response>,
  path = '/todos/settings',
) {
  const sent: Sent[] = [];
  vi.stubGlobal('fetch', async (request: Request) => {
    const text = await request.text();
    const entry = {
      method: request.method,
      url: new URL(request.url),
      headers: request.headers,
      body: text ? JSON.parse(text) : undefined,
    };
    sent.push(entry);
    return answer(entry);
  });
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={[path]}>
          <Routes>
            <Route path="/todos" element={<TodosSection />}>
              <Route path="settings" element={<CategoriesSettingsPage />} />
            </Route>
          </Routes>
          <LocationProbe />
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  const lists = () =>
    sent.filter(
      (s) => s.method === 'GET' && s.url.pathname === '/api/todos/tasks',
    );
  const writes = () => sent.filter((s) => s.method !== 'GET');
  return { sent, lists, writes };
}

const conflict = (code: string, status = 409) =>
  json({ title: 'Conflict', detail: 'Taken.', code }, status);

test('lists the categories with their active counts and adds one', async () => {
  let list = categories;
  const { writes } = renderPage((s) => {
    if (s.method === 'POST') {
      list = [
        ...list,
        {
          id: 'c-garden',
          name: 'Сад',
          emoji: '🌱',
          sortOrder: 20,
          version: 1,
          activeCount: 0,
        },
      ];
      return json({ id: 'c-garden' }, 201);
    }
    return json(list);
  });

  const rows = await screen.findByRole('list', { name: 'Категорії справ' });
  expect(rows).toHaveTextContent('Дом');
  expect(rows).toHaveTextContent('2 активні справи');
  expect(screen.getByRole('link', { name: 'Налаштування' })).toHaveAttribute(
    'aria-current',
    'page',
  );

  await userEvent.click(
    screen.getByRole('button', { name: 'Додати категорію' }),
  );
  const dialog = await screen.findByRole('dialog', { name: 'Нова категорія' });
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  expect(within(dialog).getByText('Введіть назву')).toBeInTheDocument();
  await userEvent.type(
    within(dialog).getByRole('textbox', { name: /Назва/ }),
    ' Сад ',
  );
  await userEvent.type(
    within(dialog).getByRole('textbox', { name: 'Емодзі' }),
    '🌱',
  );
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  await waitFor(() =>
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument(),
  );

  expect(writes()[0]!.body).toEqual({ name: 'Сад', emoji: '🌱' });
  expect(writes()[0]!.headers.has('Idempotency-Key')).toBe(false);
  expect(
    await screen.findByText('немає активних справ', {}, { timeout: 4000 }),
  ).toBeInTheDocument();
});

test('renames a category with its version and shows a taken name under the field', async () => {
  let taken = true;
  const { writes } = renderPage((s) => {
    if (s.method === 'PUT') {
      if (taken) {
        taken = false;
        return conflict('todos.category_name_taken');
      }
      return noContent();
    }
    return json(categories);
  });
  await screen.findByRole('list', { name: 'Категорії справ' });

  await userEvent.click(screen.getByRole('button', { name: 'Змінити: Дом' }));
  const dialog = await screen.findByRole('dialog', {
    name: 'Змінити категорію',
  });
  const name = within(dialog).getByRole('textbox', { name: /Назва/ });
  await userEvent.clear(name);
  await userEvent.type(name, 'Ремонт');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );

  expect(
    await within(dialog).findByText('Така назва вже є'),
  ).toBeInTheDocument();
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  await waitFor(() =>
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument(),
  );

  expect(writes()[1]!.url.pathname).toBe('/api/todos/categories/c-home');
  expect(writes()[1]!.headers.get('If-Match')).toBe('1');
  expect(writes()[1]!.body).toEqual({ name: 'Ремонт', emoji: '🏠' });
});

test('warns how many tasks lose a deleted category', async () => {
  const { writes } = renderPage((s) =>
    s.method === 'DELETE' ? noContent() : json(categories),
  );
  await screen.findByRole('list', { name: 'Категорії справ' });

  await userEvent.click(screen.getByRole('button', { name: 'Видалити: Дом' }));
  const dialog = await screen.findByRole('dialog', {
    name: 'Видалити категорію?',
  });
  expect(dialog).toHaveTextContent('2 справи залишаться без категорії');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Видалити' }),
  );
  await waitFor(() =>
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument(),
  );

  expect(writes()[0]!.method).toBe('DELETE');
  expect(writes()[0]!.headers.get('If-Match')).toBe('1');
});
