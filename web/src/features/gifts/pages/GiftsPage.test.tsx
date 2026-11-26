import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/query-client';
import type { GiftItem } from '../data/gifts-api';
import { GiftsSection } from '../section/GiftsSection';
import { GiftsPage } from './GiftsPage';

afterEach(() => vi.unstubAllGlobals());

const categories = [
  { id: 'c-jewel', name: 'Украшения', sortOrder: 70 },
  { id: 'c-tech', name: 'Техника', sortOrder: 10 },
];

const gift = (overrides: Partial<GiftItem> = {}): GiftItem => ({
  id: crypto.randomUUID(),
  version: 7,
  recipient: 'Maria',
  name: 'Серьги',
  categoryId: 'c-jewel',
  categoryName: 'Украшения',
  priority: 'High',
  price: null,
  link: null,
  status: 'Wanted',
  giftedOn: null,
  occasion: null,
  occasionDate: null,
  notes: null,
  variants: [],
  createdAtUtc: '2026-11-01T00:00:00Z',
  ...overrides,
});

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

function LocationProbe() {
  return <output data-testid="location">{useLocation().search}</output>;
}
const search = () => screen.getByTestId('location').textContent ?? '';

/** Stubs the API: categories always answer; `answer` handles the rest (default: the list of `items`). */
function renderPage(
  items: GiftItem[] | (() => GiftItem[]),
  answer?: (sent: Sent) => Promise<Response> | undefined,
  path = '/gifts',
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
      if (entry.url.pathname === '/api/gifts/categories')
        return json(categories);
      const custom = answer?.(entry);
      if (custom) return custom;
      if (entry.method === 'GET')
        return json(typeof items === 'function' ? items() : items);
      return json(undefined, entry.method === 'POST' ? 201 : 204);
    }),
  );
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={[path]}>
          <Routes>
            <Route path="/gifts" element={<GiftsSection />}>
              <Route index element={<GiftsPage />} />
            </Route>
          </Routes>
          <LocationProbe />
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  const lists = () =>
    sent.filter(
      (s) => s.method === 'GET' && s.url.pathname === '/api/gifts/items',
    );
  const writes = () => sent.filter((s) => s.method !== 'GET');
  return { lists, writes };
}

const three = [
  gift({ name: 'Серьги' }),
  gift({
    name: 'Наушники',
    recipient: 'Vitaliy',
    categoryId: 'c-tech',
    categoryName: 'Техника',
  }),
  gift({ name: 'Плед', recipient: 'Together', priority: 'Normal' }),
  gift({ name: 'Кольцо', priority: 'Someday' }),
];

test('shows the wanted gifts grouped by person, in the server order', async () => {
  const { lists } = renderPage(three);
  const masha = await screen.findByRole('region', { name: 'Маша' });
  expect(
    within(masha).getByRole('heading', { name: 'Маша 2' }),
  ).toBeInTheDocument();
  expect(within(masha).getAllByRole('article')).toHaveLength(2);
  const names = within(masha)
    .getAllByRole('heading', { level: 3 })
    .map((h) => h.textContent);
  expect(names).toEqual(['Серьги', 'Кольцо']);
  expect(screen.getByRole('region', { name: 'Виталий' })).toBeInTheDocument();
  expect(screen.getByRole('region', { name: 'Вместе' })).toBeInTheDocument();
  expect(lists()[0]!.url.searchParams.get('archived')).toBe('false');
});

test('filters by person and category in the URL without a new request', async () => {
  const { lists } = renderPage(three);
  await screen.findByRole('region', { name: 'Маша' });

  await userEvent.click(screen.getByRole('button', { name: 'Маша' }));
  expect(new URLSearchParams(search()).get('recipient')).toBe('Maria');
  expect(screen.queryByRole('region', { name: 'Маша' })).toBeNull();
  expect(screen.getAllByRole('article')).toHaveLength(2);

  await userEvent.click(screen.getByRole('combobox', { name: 'Категория' }));
  await userEvent.click(screen.getByRole('option', { name: 'Техника' }));
  expect(
    await screen.findByText('С этими фильтрами подарков нет'),
  ).toBeInTheDocument();
  expect(lists()).toHaveLength(1);

  await userEvent.click(
    screen.getByRole('button', { name: 'Сбросить фильтры' }),
  );
  expect(screen.getAllByRole('article')).toHaveLength(4);
  expect(search()).toBe('');
});

test('ignores a person or category in the link that does not exist', async () => {
  renderPage(three, undefined, '/gifts?recipient=Bob&category=nope');
  expect(
    await screen.findByRole('region', { name: 'Маша' }),
  ).toBeInTheDocument();
  await waitFor(() => expect(screen.getAllByRole('article')).toHaveLength(4));
});

test('says when the wishlist is empty', async () => {
  renderPage([]);
  expect(await screen.findByText('Список желаний пуст')).toBeInTheDocument();
  expect(
    screen.getByRole('button', { name: 'Добавить подарок' }),
  ).toBeInTheDocument();
});

test('shows a failed load and retries', async () => {
  let fail = true;
  renderPage(three, (sent) =>
    sent.method === 'GET' && fail
      ? json({ detail: 'База недоступна' }, 503)
      : undefined,
  );
  expect(
    await screen.findByText(/Не удалось загрузить: База недоступна/),
  ).toBeInTheDocument();
  fail = false;
  await userEvent.click(screen.getByRole('button', { name: 'Повторить' }));
  expect(
    await screen.findByRole('region', { name: 'Маша' }),
  ).toBeInTheDocument();
});

test('adds a gift with variants, preset from the filter, with one key per data', async () => {
  let fail = true;
  const { writes } = renderPage(
    [],
    (sent) =>
      sent.method === 'POST' && fail ? json({ detail: 'Ой' }, 500) : undefined,
    '/gifts?recipient=Maria',
  );
  await screen.findByText('Список желаний пуст');
  await userEvent.click(
    screen.getByRole('button', { name: 'Добавить подарок' }),
  );
  const dialog = screen.getByRole('dialog', { name: 'Новый подарок' });
  expect(
    within(dialog).getByRole('combobox', { name: 'Для кого' }),
  ).toHaveTextContent('Маша');
  expect(
    within(dialog).getByRole('combobox', { name: 'Приоритет' }),
  ).toHaveTextContent('⭐ Хочу');

  await userEvent.type(within(dialog).getByLabelText('Название'), 'Серьги');
  await userEvent.click(
    within(dialog).getByRole('combobox', { name: 'Категория' }),
  );
  await userEvent.click(screen.getByRole('option', { name: 'Украшения' }));
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Добавить вариант' }),
  );
  expect(within(dialog).getByLabelText('Ссылка варианта 1')).toHaveFocus();
  await userEvent.type(
    within(dialog).getByLabelText('Ссылка варианта 1'),
    'https://a.ua/1',
  );
  await userEvent.type(
    within(dialog).getByLabelText('Название варианта'),
    'Голубой',
  );
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Добавить вариант' }),
  );
  await userEvent.type(
    within(dialog).getByLabelText('Ссылка варианта 2'),
    'https://a.ua/2',
  );
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );

  expect(
    await within(dialog).findByText('Не удалось сохранить: Ой'),
  ).toBeInTheDocument();
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );
  await waitFor(() => expect(writes()).toHaveLength(2));
  const [first, second] = writes();
  expect(second!.headers.get('Idempotency-Key')).toBe(
    first!.headers.get('Idempotency-Key'),
  );

  fail = false;
  await userEvent.type(
    within(dialog).getByLabelText('Заметки'),
    'Белое золото',
  );
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );
  await waitFor(() => expect(screen.queryByRole('dialog')).toBeNull());
  const third = writes()[2]!;
  expect(third.headers.get('Idempotency-Key')).not.toBe(
    first!.headers.get('Idempotency-Key'),
  );
  expect(third.url.pathname).toBe('/api/gifts/items');
  expect(third.body).toMatchObject({
    recipient: 'Maria',
    name: 'Серьги',
    categoryId: 'c-jewel',
    priority: 'Normal',
    price: null,
    notes: 'Белое золото',
    variants: [
      { title: 'Голубой', link: 'https://a.ua/1', price: null },
      { title: null, link: 'https://a.ua/2', price: null },
    ],
  });
});

test('checks the fields before sending', async () => {
  const { writes } = renderPage([]);
  await screen.findByText('Список желаний пуст');
  await userEvent.click(
    screen.getByRole('button', { name: 'Добавить подарок' }),
  );
  const dialog = screen.getByRole('dialog');
  await userEvent.type(within(dialog).getByLabelText('Ссылка'), 'shop.ua');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );
  for (const text of [
    'Выберите, для кого',
    'Введите название',
    'Выберите категорию',
    'Ссылка должна начинаться с http:// или https://',
  ])
    expect(within(dialog).getByText(text)).toBeInTheDocument();
  expect(writes()).toHaveLength(0);
});

test('edits a gift with its version and replaces its variants', async () => {
  const item = gift({
    name: 'Наушники',
    variants: [
      { id: 'v1', title: 'A', link: 'https://a.ua/1', price: null },
      { id: 'v2', title: 'B', link: 'https://a.ua/2', price: null },
    ],
  });
  const { writes } = renderPage([item]);
  await userEvent.click(
    await screen.findByRole('button', { name: 'Действия: Наушники' }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Изменить' }));
  const dialog = screen.getByRole('dialog', { name: 'Изменить подарок' });
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Удалить вариант 1' }),
  );
  await userEvent.type(
    within(dialog).getByLabelText('Цена (если известна)'),
    '1 500',
  );
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );

  await waitFor(() => expect(writes()).toHaveLength(1));
  const put = writes()[0]!;
  expect(put.method).toBe('PUT');
  expect(put.url.pathname).toBe(`/api/gifts/items/${item.id}`);
  expect(put.headers.get('If-Match')).toBe('7');
  expect(put.body).toMatchObject({
    price: 1500,
    variants: [{ title: 'B', link: 'https://a.ua/2', price: null }],
  });
});

test('says when a gift was changed elsewhere and reloads the list', async () => {
  const { lists } = renderPage([gift({ name: 'Плед' })], (sent) =>
    sent.method === 'PUT'
      ? json({ detail: 'x', code: 'gifts.changed_meanwhile' }, 409)
      : undefined,
  );
  await userEvent.click(
    await screen.findByRole('button', { name: 'Действия: Плед' }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Изменить' }));
  await userEvent.click(screen.getByRole('button', { name: 'Сохранить' }));
  expect(
    await screen.findByText(/изменили в другом месте/),
  ).toBeInTheDocument();
  await waitFor(() => expect(lists().length).toBeGreaterThan(1));
});

test('deletes a gift after asking', async () => {
  const item = gift({ name: 'Плед' });
  const { writes } = renderPage([item]);
  await userEvent.click(
    await screen.findByRole('button', { name: 'Действия: Плед' }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Удалить' }));
  const dialog = screen.getByRole('dialog', { name: 'Удалить подарок?' });
  expect(within(dialog).getByText('Плед · Маша')).toBeInTheDocument();
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Удалить' }),
  );
  await waitFor(() => expect(screen.queryByRole('dialog')).toBeNull());
  expect(writes()[0]).toMatchObject({ method: 'DELETE' });
  expect(writes()[0]!.url.pathname).toBe(`/api/gifts/items/${item.id}`);
});

test('marks a gift gifted and it leaves the list', async () => {
  const item = gift({ name: 'Плед' });
  let gone = false;
  const { writes } = renderPage(
    () => (gone ? [] : [item]),
    (sent) => {
      if (sent.method !== 'PUT') return undefined;
      gone = true;
      return json(undefined, 204);
    },
  );
  await userEvent.click(
    await screen.findByRole('button', { name: 'Подарено' }),
  );
  expect(await screen.findByText('Список желаний пуст')).toBeInTheDocument();
  const put = writes()[0]!;
  expect(put.url.pathname).toBe(`/api/gifts/items/${item.id}/status`);
  expect(put.headers.get('If-Match')).toBe('7');
  expect(put.body).toEqual({ status: 'Gifted', giftedOn: null });
});

test('marks a gift not relevant, and says when writes are too frequent', async () => {
  const { writes } = renderPage([gift({ name: 'Плед' })], (sent) =>
    sent.method === 'PUT' ? json({ detail: 'x' }, 429) : undefined,
  );
  await userEvent.click(
    await screen.findByRole('button', { name: 'Действия: Плед' }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Не актуально' }));
  expect(
    await screen.findByText(
      'Слишком много изменений подряд. Подождите минуту и повторите.',
    ),
  ).toBeInTheDocument();
  expect(writes()[0]!.body).toEqual({ status: 'Dropped', giftedOn: null });
});
