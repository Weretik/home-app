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
  giver: null,
  buyBy: null,
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

const rows = () => screen.getAllByRole('listitem');
const rowNames = (scope: HTMLElement) =>
  within(scope)
    .getAllByRole('listitem')
    .map((row) => within(row).getAllByText(/^[А-Яа-я ]+$/)[0]!.textContent);

test('shows the wanted gifts in a block per person, in the server order', async () => {
  const { lists } = renderPage(three);
  const masha = await screen.findByRole('region', { name: 'Маша' });
  expect(
    within(masha).getByRole('heading', { name: 'Маша' }),
  ).toBeInTheDocument();
  expect(within(masha).getByText(/^2 подарунки/)).toBeInTheDocument();
  expect(rowNames(masha)).toEqual(['Серьги', 'Кольцо']);
  expect(screen.getByRole('region', { name: 'Віталій' })).toBeInTheDocument();
  expect(screen.getByRole('region', { name: 'Разом' })).toBeInTheDocument();
  expect(lists()[0]!.url.searchParams.get('archived')).toBe('false');
});

test('filters by person and category in the URL without a new request', async () => {
  const { lists } = renderPage(three);
  await screen.findByRole('region', { name: 'Маша' });

  await userEvent.click(screen.getByRole('button', { name: 'Маша' }));
  expect(new URLSearchParams(search()).get('recipient')).toBe('Maria');
  expect(screen.queryByRole('region', { name: 'Маша' })).toBeNull();
  expect(
    within(
      screen.getByRole('list', { name: 'Список подарунків' }),
    ).getAllByRole('listitem'),
  ).toHaveLength(2);

  await userEvent.click(screen.getByRole('combobox', { name: 'Категорія' }));
  await userEvent.click(screen.getByRole('option', { name: 'Техника' }));
  expect(
    await screen.findByText('З цими фільтрами подарунків немає'),
  ).toBeInTheDocument();
  expect(lists()).toHaveLength(1);

  await userEvent.click(
    screen.getByRole('button', { name: 'Скинути фільтри' }),
  );
  expect(rows()).toHaveLength(4);
  expect(search()).toBe('');
});

test('ignores a person or category in the link that does not exist', async () => {
  renderPage(three, undefined, '/gifts?recipient=Bob&category=nope');
  expect(
    await screen.findByRole('region', { name: 'Маша' }),
  ).toBeInTheDocument();
  await waitFor(() => expect(rows()).toHaveLength(4));
});

test('says when the wishlist is empty', async () => {
  renderPage([]);
  expect(await screen.findByText('Список бажань порожній')).toBeInTheDocument();
  expect(
    screen.getByRole('button', { name: 'Додати подарунок' }),
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
    await screen.findByText(/Не вдалося завантажити: База недоступна/),
  ).toBeInTheDocument();
  fail = false;
  await userEvent.click(screen.getByRole('button', { name: 'Повторити' }));
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
  await screen.findByText('Список бажань порожній');
  await userEvent.click(
    screen.getByRole('button', { name: 'Додати подарунок' }),
  );
  const dialog = screen.getByRole('dialog', { name: 'Новий подарунок' });
  expect(
    within(dialog).getByRole('combobox', { name: 'Для кого' }),
  ).toHaveTextContent('Маша');
  expect(
    within(dialog).getByRole('combobox', { name: 'Пріоритет' }),
  ).toHaveTextContent('⭐ Хочу');

  await userEvent.type(within(dialog).getByLabelText('Назва'), 'Серьги');
  await userEvent.click(
    within(dialog).getByRole('combobox', { name: 'Категорія' }),
  );
  await userEvent.click(screen.getByRole('option', { name: 'Украшения' }));
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Додати варіант' }),
  );
  expect(within(dialog).getByLabelText('Посилання варіанта 1')).toHaveFocus();
  await userEvent.type(
    within(dialog).getByLabelText('Посилання варіанта 1'),
    'https://a.ua/1',
  );
  await userEvent.type(
    within(dialog).getByLabelText('Назва варіанта'),
    'Голубой',
  );
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Додати варіант' }),
  );
  await userEvent.type(
    within(dialog).getByLabelText('Посилання варіанта 2'),
    'https://a.ua/2',
  );
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );

  expect(
    await within(dialog).findByText('Не вдалося зберегти: Ой'),
  ).toBeInTheDocument();
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  await waitFor(() => expect(writes()).toHaveLength(2));
  const [first, second] = writes();
  expect(second!.headers.get('Idempotency-Key')).toBe(
    first!.headers.get('Idempotency-Key'),
  );

  fail = false;
  await userEvent.type(
    within(dialog).getByLabelText('Нотатки'),
    'Белое золото',
  );
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
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
  await screen.findByText('Список бажань порожній');
  await userEvent.click(
    screen.getByRole('button', { name: 'Додати подарунок' }),
  );
  const dialog = screen.getByRole('dialog');
  await userEvent.type(within(dialog).getByLabelText('Посилання'), 'shop.ua');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  for (const text of [
    'Виберіть, для кого',
    'Введіть назву',
    'Виберіть категорію',
    'Посилання має починатися з http:// або https://',
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
    await screen.findByRole('button', { name: 'Дії: Наушники' }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Змінити' }));
  const dialog = screen.getByRole('dialog', { name: 'Змінити подарунок' });
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Видалити варіант 1' }),
  );
  await userEvent.type(
    within(dialog).getByLabelText('Ціна (якщо відома)'),
    '1 500',
  );
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
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
    await screen.findByRole('button', { name: 'Дії: Плед' }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Змінити' }));
  await userEvent.click(screen.getByRole('button', { name: 'Зберегти' }));
  expect(await screen.findByText(/змінили в іншому місці/)).toBeInTheDocument();
  await waitFor(() => expect(lists().length).toBeGreaterThan(1));
});

test('deletes a gift after asking', async () => {
  const item = gift({ name: 'Плед' });
  const { writes } = renderPage([item]);
  await userEvent.click(
    await screen.findByRole('button', { name: 'Дії: Плед' }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Видалити' }));
  const dialog = screen.getByRole('dialog', { name: 'Видалити подарунок?' });
  expect(within(dialog).getByText('Плед · Маша')).toBeInTheDocument();
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Видалити' }),
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
    await screen.findByRole('button', { name: 'Дії: Плед' }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Подаровано' }));
  expect(await screen.findByText('Список бажань порожній')).toBeInTheDocument();
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
    await screen.findByRole('button', { name: 'Дії: Плед' }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Неактуально' }));
  expect(
    await screen.findByText(
      'Забагато змін поспіль. Зачекайте хвилину й повторіть.',
    ),
  ).toBeInTheDocument();
  expect(writes()[0]!.body).toEqual({ status: 'Dropped', giftedOn: null });
});
