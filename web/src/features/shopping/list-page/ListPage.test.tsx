import { ThemeProvider } from '@mui/material/styles';
import { onlineManager, QueryClientProvider } from '@tanstack/react-query';
import { act, render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/client/query-client';
import { ShoppingSection } from '../section/ShoppingSection';
import { ListPage } from './ListPage';

afterEach(() => {
  vi.unstubAllGlobals();
  onlineManager.setOnline(true);
  localStorage.clear();
  sessionStorage.clear();
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
      // A refusal: a queued write retries a 5xx a few times, a 4xx never.
      if (options.failStatus)
        return json(
          { title: 'Помилка', detail: 'Сервер відмовив', status: 400 },
          400,
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
  expect(rows[0]).toHaveTextContent('Терміново');
  expect(rows[0]).toHaveTextContent('для пасты');
  expect(rows[1]).toHaveTextContent('Молоко2 л');
  expect(rows[1]).toHaveTextContent('додав(ла) Маша');
  const regions = screen
    .getAllByRole('region')
    .map((r) => r.getAttribute('aria-label') ?? r.textContent?.slice(0, 12));
  expect(regions.findIndex((r) => r?.includes('Молочка'))).toBeLessThan(
    regions.findIndex((r) => r?.includes('Хлеб')),
  );
  expect(within(group('У кошику')).getByText('Кефир')).toBeInTheDocument();
  expect(
    screen.getByText(/Потрібно 3 · терміново 1 · у кошику 1/),
  ).toBeInTheDocument();
});

test('a tick moves the item to the cart at once and sends who bought it', async () => {
  const { writes } = renderList();
  await screen.findByRole('region', { name: /Молочка/ });

  await userEvent.click(screen.getByRole('checkbox', { name: 'Молоко' }));

  expect(within(group('У кошику')).getByText('Молоко')).toBeInTheDocument();
  await waitFor(() => expect(writes()).toHaveLength(1));
  expect(writes()[0]!.url.pathname).toBe('/api/shopping/items/milk/status');
  expect(writes()[0]!.body).toEqual({ status: 'Bought', by: 'Vitaliy' });
});

test('offline, ticks wait in the queue on the screen and are sent in order when the network is back', async () => {
  const { writes } = renderList();
  await screen.findByRole('region', { name: /Молочка/ });
  onlineManager.setOnline(false);

  await userEvent.click(screen.getByRole('checkbox', { name: 'Молоко' }));
  await userEvent.click(screen.getByRole('checkbox', { name: 'Кефир' }));

  expect(within(group('У кошику')).getByText('Молоко')).toBeInTheDocument();
  expect(writes()).toHaveLength(0);

  act(() => onlineManager.setOnline(true));

  await waitFor(() => expect(writes()).toHaveLength(2));
  expect(writes().map((w) => w.url.pathname)).toEqual([
    '/api/shopping/items/milk/status',
    '/api/shopping/items/kefir/status',
  ]);
});

test('offline, the quick add clears the field at once and says it will be added', async () => {
  const { writes } = renderList();
  await screen.findByRole('region', { name: /Молочка/ });
  onlineManager.setOnline(false);
  const field = screen.getByRole('combobox', { name: 'Що купити' });

  await userEvent.type(field, 'творог{Enter}');

  expect(field).toHaveValue('');
  expect(
    await screen.findByText('Додасться, коли з’явиться мережа'),
  ).toBeInTheDocument();
  expect(writes()).toHaveLength(0);

  act(() => onlineManager.setOnline(true));

  await waitFor(() => expect(writes()).toHaveLength(1));
  expect(writes()[0]!.headers.get('Idempotency-Key')).toBeTruthy();
  expect(writes()[0]!.body).toMatchObject({ text: 'творог' });
});

test('a failed tick puts the item back and says why', async () => {
  renderList({ failStatus: true });
  await screen.findByRole('region', { name: /Молочка/ });

  await userEvent.click(screen.getByRole('checkbox', { name: 'Молоко' }));

  expect(await screen.findByText(/Не вдалося позначити/)).toBeInTheDocument();
  expect(
    within(group('Молочка')).getByRole('checkbox', { name: 'Молоко' }),
  ).not.toBeChecked();
});

test('adds from the field as «Я» with an Idempotency-Key, then «Скасувати» deletes the added row', async () => {
  const { writes } = renderList();
  await screen.findByRole('region', { name: /Молочка/ });
  await userEvent.click(screen.getByRole('button', { name: 'Маша' }));

  await userEvent.type(
    screen.getByRole('combobox', { name: 'Що купити' }),
    'творог{Enter}',
  );

  expect(await screen.findByText('Додано: Творог')).toBeInTheDocument();
  const [post] = writes();
  expect(post!.headers.get('Idempotency-Key')).toBeTruthy();
  expect(post!.body).toEqual({
    text: 'творог',
    addedBy: 'Maria',
    departmentId: null,
    urgent: false,
  });
  expect(screen.getByRole('combobox', { name: 'Що купити' })).toHaveValue('');

  await userEvent.click(screen.getByRole('button', { name: 'Скасувати' }));
  await waitFor(() => expect(writes()).toHaveLength(2));
  expect(writes()[1]!.method).toBe('DELETE');
  expect(writes()[1]!.url.pathname).toBe('/api/shopping/items/new');
  expect(writes()[1]!.headers.get('If-Match')).toBe('9');
});

test('shows the department of a single typed item and adds a frequent one with its department', async () => {
  const { writes } = renderList();
  await screen.findByRole('region', { name: /Молочка/ });

  await userEvent.type(
    screen.getByRole('combobox', { name: 'Що купити' }),
    'йогурт',
  );
  expect(
    await screen.findByText(/Відділ: 🥛 Молочка/, {}, { timeout: 2000 }),
  ).toBeInTheDocument();

  await userEvent.click(screen.getByRole('button', { name: 'Додати Батон' }));
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
    screen.getByRole('button', { name: 'Змінити: Молоко' }),
  );
  const dialog = await screen.findByRole('dialog', { name: 'Змінити товар' });
  const quantity = within(dialog).getByLabelText('Кількість');
  await userEvent.clear(quantity);
  await userEvent.type(quantity, '3 л');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
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
    screen.getByRole('button', { name: 'Прибрати куплене' }),
  );
  await waitFor(() => expect(writes()).toHaveLength(2));
  expect(writes()[1]!.url.pathname).toBe('/api/shopping/items/clear-cart');
});

test('an empty list says so', async () => {
  renderList({ items: [] });

  expect(await screen.findByText('Список порожній 🎉')).toBeInTheDocument();
});
