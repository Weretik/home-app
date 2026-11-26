import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/query-client';
import { ShoppingSection } from '../section/ShoppingSection';
import { ListPage } from './ListPage';

afterEach(() => {
  vi.unstubAllGlobals();
  localStorage.clear();
});

const dairy = {
  departmentId: 'd-dairy',
  departmentName: 'Молочка',
  departmentIcon: '🥛',
  departmentSortOrder: 1,
};
const bakery = {
  departmentId: 'd-bread',
  departmentName: 'Хлеб',
  departmentIcon: '🍞',
  departmentSortOrder: 2,
};
const base = {
  quantity: null,
  note: null,
  urgent: false,
  status: 'Needed',
  addedBy: 'Maria',
  boughtBy: null,
  boughtAtUtc: null,
  version: 4,
};
const fixture = () => [
  {
    ...base,
    ...bakery,
    id: 'bread',
    name: 'Хлеб',
    addedAtUtc: '2026-11-26T07:00:00Z',
  },
  {
    ...base,
    ...dairy,
    id: 'milk',
    name: 'Молоко',
    quantity: '2 л',
    addedAtUtc: '2026-11-26T08:00:00Z',
  },
  {
    ...base,
    ...dairy,
    id: 'cheese',
    name: 'Сыр',
    urgent: true,
    note: 'для пасты',
    addedAtUtc: '2026-11-26T09:00:00Z',
  },
  {
    ...base,
    ...dairy,
    id: 'kefir',
    name: 'Кефир',
    status: 'Bought',
    boughtBy: 'Vitaliy',
    boughtAtUtc: '2026-11-26T09:30:00Z',
    addedAtUtc: '2026-11-26T06:00:00Z',
  },
];
const departments = [
  {
    id: 'd-dairy',
    name: 'Молочка',
    icon: '🥛',
    sortOrder: 1,
    isFallback: false,
    keywordCount: 10,
    neededCount: 2,
    version: 1,
  },
  {
    id: 'd-bread',
    name: 'Хлеб',
    icon: '🍞',
    sortOrder: 2,
    isFallback: false,
    keywordCount: 5,
    neededCount: 1,
    version: 1,
  },
];

const json = (body: unknown, status = 200) =>
  Promise.resolve(
    new Response(JSON.stringify(body), {
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

function renderList(options: { items?: unknown[]; failStatus?: boolean } = {}) {
  const sent: Sent[] = [];
  let items = (options.items ?? fixture()) as Array<Record<string, unknown>>;
  vi.stubGlobal('fetch', async (request: Request) => {
    const text = await request.text();
    const entry: Sent = {
      method: request.method,
      url: new URL(request.url),
      headers: request.headers,
      body: text ? JSON.parse(text) : undefined,
    };
    sent.push(entry);
    const path = entry.url.pathname;
    if (entry.method === 'GET') {
      if (path === '/api/shopping/items') return json({ items });
      if (path === '/api/shopping/frequent')
        return json([
          {
            ...bakery,
            name: 'Батон',
            timesBought: 7,
            lastBoughtAtUtc: '2026-11-20T10:00:00Z',
          },
        ]);
      if (path === '/api/shopping/departments/match')
        return json({
          departmentId: 'd-dairy',
          departmentName: 'Молочка',
          departmentIcon: '🥛',
        });
      if (path === '/api/shopping/departments') return json(departments);
    }
    if (path.endsWith('/status')) {
      if (options.failStatus)
        return json(
          { title: 'Ошибка', detail: 'Сервер упал', status: 500 },
          500,
        );
      const id = path.split('/')[4];
      const { status, by } = entry.body as { status: string; by: string };
      items = items.map((i) =>
        i.id === id ? { ...i, status, boughtBy: by } : i,
      );
      return json(items.find((i) => i.id === id));
    }
    if (entry.method === 'POST' && path === '/api/shopping/items') {
      const added = {
        ...base,
        ...dairy,
        id: 'new',
        name: 'Творог',
        addedAtUtc: '2026-11-26T10:00:00Z',
        version: 9,
      };
      return json({ added: [added], restored: [], updated: [] }, 201);
    }
    if (path === '/api/shopping/items/clear-cart') return json({ cleared: 1 });
    if (entry.method === 'PUT') return json(items[0]);
    return new Response(null, { status: 204 });
  });
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={['/shopping']}>
          <Routes>
            <Route path="/shopping" element={<ShoppingSection />}>
              <Route index element={<ListPage />} />
            </Route>
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  return { writes: () => sent.filter((s) => s.method !== 'GET'), sent };
}

const group = (name: string) =>
  screen.getByRole('region', { name: new RegExp(name) });

test('shows departments in store order with urgent items first, the cart and the counts', async () => {
  renderList();

  const dairyGroup = await screen.findByRole('region', { name: /Молочка/ });
  const rows = within(dairyGroup).getAllByRole('listitem');
  expect(
    rows.map((r) =>
      within(r).getByRole('checkbox').getAttribute('aria-labelledby'),
    ),
  ).toHaveLength(2);
  expect(rows[0]).toHaveTextContent('Сыр');
  expect(rows[0]).toHaveTextContent('Срочно');
  expect(rows[0]).toHaveTextContent('для пасты');
  expect(rows[1]).toHaveTextContent('Молоко2 л');
  expect(rows[1]).toHaveTextContent('добавил(а) Маша');
  const regions = screen
    .getAllByRole('region')
    .map((r) => r.getAttribute('aria-label') ?? r.textContent?.slice(0, 12));
  expect(regions.findIndex((r) => r?.includes('Молочка'))).toBeLessThan(
    regions.findIndex((r) => r?.includes('Хлеб')),
  );
  expect(within(group('В корзине')).getByText('Кефир')).toBeInTheDocument();
  expect(
    screen.getByText(/Нужно 3 · срочно 1 · в корзине 1/),
  ).toBeInTheDocument();
});

test('a tick moves the item to the cart at once and sends who bought it', async () => {
  const { writes } = renderList();
  await screen.findByRole('region', { name: /Молочка/ });

  await userEvent.click(screen.getByRole('checkbox', { name: 'Молоко' }));

  expect(within(group('В корзине')).getByText('Молоко')).toBeInTheDocument();
  await waitFor(() => expect(writes()).toHaveLength(1));
  expect(writes()[0]!.url.pathname).toBe('/api/shopping/items/milk/status');
  expect(writes()[0]!.body).toEqual({ status: 'Bought', by: 'Vitaliy' });
});

test('a failed tick puts the item back and says why', async () => {
  renderList({ failStatus: true });
  await screen.findByRole('region', { name: /Молочка/ });

  await userEvent.click(screen.getByRole('checkbox', { name: 'Молоко' }));

  expect(await screen.findByText(/Не удалось отметить/)).toBeInTheDocument();
  expect(
    within(group('Молочка')).getByRole('checkbox', { name: 'Молоко' }),
  ).not.toBeChecked();
});

test('adds from the field as «Я» with an Idempotency-Key, then «Отменить» deletes the added row', async () => {
  const { writes } = renderList();
  await screen.findByRole('region', { name: /Молочка/ });
  await userEvent.click(screen.getByRole('button', { name: 'Маша' }));

  await userEvent.type(
    screen.getByRole('combobox', { name: 'Что купить' }),
    'творог{Enter}',
  );

  expect(await screen.findByText('Добавлено: Творог')).toBeInTheDocument();
  const [post] = writes();
  expect(post!.headers.get('Idempotency-Key')).toBeTruthy();
  expect(post!.body).toEqual({
    text: 'творог',
    addedBy: 'Maria',
    departmentId: null,
    urgent: false,
  });
  expect(screen.getByRole('combobox', { name: 'Что купить' })).toHaveValue('');

  await userEvent.click(screen.getByRole('button', { name: 'Отменить' }));
  await waitFor(() => expect(writes()).toHaveLength(2));
  expect(writes()[1]!.method).toBe('DELETE');
  expect(writes()[1]!.url.pathname).toBe('/api/shopping/items/new');
  expect(writes()[1]!.headers.get('If-Match')).toBe('9');
});

test('shows the department of a single typed item and adds a frequent one with its department', async () => {
  const { writes } = renderList();
  await screen.findByRole('region', { name: /Молочка/ });

  await userEvent.type(
    screen.getByRole('combobox', { name: 'Что купить' }),
    'йогурт',
  );
  expect(
    await screen.findByText(/Отдел: 🥛 Молочка/, {}, { timeout: 2000 }),
  ).toBeInTheDocument();

  await userEvent.click(screen.getByRole('button', { name: 'Добавить Батон' }));
  await waitFor(() => expect(writes()).toHaveLength(1));
  expect(writes()[0]!.body).toEqual({
    text: 'Батон',
    addedBy: 'Vitaliy',
    departmentId: 'd-bread',
    urgent: false,
  });
});

test('edits an item with its version and clears the cart', async () => {
  const { writes } = renderList();
  await screen.findByRole('region', { name: /Молочка/ });

  await userEvent.click(
    screen.getByRole('button', { name: 'Изменить: Молоко' }),
  );
  const dialog = await screen.findByRole('dialog', { name: 'Изменить товар' });
  const quantity = within(dialog).getByLabelText('Количество');
  await userEvent.clear(quantity);
  await userEvent.type(quantity, '3 л');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );
  await waitFor(() =>
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument(),
  );

  const [put] = writes();
  expect(put!.url.pathname).toBe('/api/shopping/items/milk');
  expect(put!.headers.get('If-Match')).toBe('4');
  expect(put!.body).toEqual({
    name: 'Молоко',
    quantity: '3 л',
    note: null,
    departmentId: 'd-dairy',
    urgent: false,
  });

  await userEvent.click(
    screen.getByRole('button', { name: 'Убрать купленные' }),
  );
  await waitFor(() => expect(writes()).toHaveLength(2));
  expect(writes()[1]!.url.pathname).toBe('/api/shopping/items/clear-cart');
});

test('an empty list says so', async () => {
  renderList({ items: [] });

  expect(await screen.findByText('Список пуст 🎉')).toBeInTheDocument();
});
