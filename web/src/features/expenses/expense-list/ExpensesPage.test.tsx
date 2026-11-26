import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/query-client';
import { currentMonth, formatMonth } from '../data/month';
import { ExpensesSection } from '../section/ExpensesSection';
import { ExpensesPage } from './ExpensesPage';

afterEach(() => vi.unstubAllGlobals());

const categories = [
  { id: 'c-food', name: 'Продукты', sortOrder: 1 },
  { id: 'c-home', name: 'Дом', sortOrder: 2 },
];

const expense = (overrides: Record<string, unknown> = {}) => ({
  id: crypto.randomUUID(),
  version: 1,
  date: '2026-10-01',
  payer: 'Shared',
  categoryId: 'c-food',
  categoryName: 'Продукты',
  amount: 1234.5,
  paymentMethod: 'Card',
  comment: 'Сильпо',
  ...overrides,
});

const page = (items: unknown[], extra: Record<string, unknown> = {}) => ({
  items,
  page: 1,
  pageSize: 50,
  totalCount: items.length,
  totalAmount: 95024,
  ...extra,
});

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

/** Stubs the API: categories always answer; `expenses` answers the list for each request URL. */
function renderSection(
  expenses: (url: URL, sent: Sent) => Promise<Response>,
  path = '/expenses?year=2026&month=10',
) {
  const sent: Sent[] = [];
  const fetchMock = vi.fn(async (input: Request | string) => {
    const request = typeof input === 'string' ? new Request(input) : input;
    const text = await request.text();
    const entry: Sent = {
      method: request.method,
      url: new URL(request.url),
      headers: request.headers,
      body: text ? JSON.parse(text) : undefined,
    };
    sent.push(entry);
    return entry.url.pathname === '/api/expenses/categories'
      ? json(categories)
      : expenses(entry.url, entry);
  });
  vi.stubGlobal('fetch', fetchMock);
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={[path]}>
          <Routes>
            <Route path="/expenses" element={<ExpensesSection />}>
              <Route index element={<ExpensesPage />} />
            </Route>
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  const expenseUrls = () =>
    sent
      .filter((s) => s.method === 'GET' && s.url.pathname === '/api/expenses')
      .map((s) => s.url);
  const writes = () => sent.filter((s) => s.method !== 'GET');
  return { expenseUrls, writes };
}

const normalized = (text: string) => (_: string, element: Element | null) =>
  element?.textContent?.replace(/\s/g, ' ') === text &&
  !Array.from(element.children).some(
    (child) => child.textContent?.replace(/\s/g, ' ') === text,
  );

test('shows each expense with Russian labels and no month total card', async () => {
  const { expenseUrls } = renderSection(() =>
    json(
      page([
        expense(),
        expense({ payer: 'Maria', paymentMethod: 'Cash', comment: null }),
      ]),
    ),
  );

  const list = await screen.findByRole('list', { name: 'Расходы за месяц' });
  expect(screen.getByText('Октябрь 2026')).toBeInTheDocument();
  // The total card is gone; an overall summary will be its own page.
  expect(screen.queryByText(normalized('95 024,00 ₴'))).not.toBeInTheDocument();
  expect(screen.queryByText('2 записи')).not.toBeInTheDocument();
  expect(
    screen.queryByRole('region', { name: 'Расходы за месяц' }),
  ).not.toBeInTheDocument();

  const rows = within(list).getAllByRole('listitem');
  expect(rows).toHaveLength(2);
  expect(rows[0]).toHaveTextContent('Общее');
  expect(rows[0]).toHaveTextContent('Карта');
  expect(rows[0]).toHaveTextContent('Сильпо');
  expect(rows[1]).toHaveTextContent('Маша');
  expect(rows[1]).toHaveTextContent('Наличные');
  expect(
    within(rows[0]!).getByText(normalized('1 234,50 ₴')),
  ).toBeInTheDocument();

  const query = expenseUrls()[0]!.searchParams;
  expect(query.get('year')).toBe('2026');
  expect(query.get('month')).toBe('10');
  expect(query.get('page')).toBe('1');
  expect(query.get('pageSize')).toBe('50');
});

test('opens the current month when the URL has none', async () => {
  renderSection(() => json(page([])), '/expenses');

  expect(
    await screen.findByText(formatMonth(currentMonth())),
  ).toBeInTheDocument();
});

test('moves to the previous month and keeps it for the incomes tab', async () => {
  const { expenseUrls } = renderSection(() => json(page([expense()])));
  await screen.findByRole('list', { name: 'Расходы за месяц' });

  await userEvent.click(
    screen.getByRole('button', { name: 'Предыдущий месяц' }),
  );

  expect(await screen.findByText('Сентябрь 2026')).toBeInTheDocument();
  expect(expenseUrls().at(-1)!.searchParams.get('month')).toBe('9');
  expect(screen.getByRole('tab', { name: 'Доходы' })).toHaveAttribute(
    'href',
    '/expenses/incomes?year=2026&month=9',
  );
});

test('filters by who paid and by category', async () => {
  const { expenseUrls } = renderSection(() => json(page([expense()])));
  await screen.findByRole('list', { name: 'Расходы за месяц' });

  await userEvent.click(screen.getByRole('combobox', { name: 'Кто' }));
  await userEvent.click(screen.getByRole('option', { name: 'Общее' }));
  await userEvent.click(screen.getByRole('combobox', { name: 'Категория' }));
  await userEvent.click(await screen.findByRole('option', { name: 'Дом' }));

  const query = expenseUrls().at(-1)!.searchParams;
  expect(query.get('payer')).toBe('Shared');
  expect(query.get('categoryId')).toBe('c-home');
});

test('says when the month has no expenses', async () => {
  renderSection(() => json(page([], { totalAmount: 0 })));

  expect(
    await screen.findByText('За этот месяц записей нет'),
  ).toBeInTheDocument();
});

test('offers to reset a filter that leaves nothing', async () => {
  const { expenseUrls } = renderSection(
    () => json(page([], { totalAmount: 0 })),
    '/expenses?year=2026&month=10&payer=Maria',
  );

  expect(
    await screen.findByText('С этим фильтром записей нет'),
  ).toBeInTheDocument();
  await userEvent.click(
    screen.getByRole('button', { name: 'Сбросить фильтр' }),
  );

  await screen.findByText('За этот месяц записей нет');
  expect(expenseUrls().at(-1)!.searchParams.has('payer')).toBe(false);
});

test('shows the error and retries', async () => {
  let fail = true;
  renderSection(() =>
    fail ? json({ title: 'Ошибка сервера' }, 500) : json(page([expense()])),
  );

  expect(await screen.findByRole('alert')).toHaveTextContent(
    'Не удалось загрузить',
  );
  fail = false;
  await userEvent.click(screen.getByRole('button', { name: 'Повторить' }));

  expect(
    await screen.findByRole('list', { name: 'Расходы за месяц' }),
  ).toBeInTheDocument();
});

test('loads the next page on «Показать ещё»', async () => {
  const { expenseUrls } = renderSection((url) =>
    url.searchParams.get('page') === '1'
      ? json(page([expense()], { pageSize: 1, totalCount: 2 }))
      : json(
          page([expense({ categoryName: 'Дом' })], {
            page: 2,
            pageSize: 1,
            totalCount: 2,
          }),
        ),
  );

  await userEvent.click(
    await screen.findByRole('button', { name: 'Показать ещё' }),
  );

  expect(await screen.findByText('Дом')).toBeInTheDocument();
  expect(expenseUrls().at(-1)!.searchParams.get('page')).toBe('2');
  expect(
    screen.queryByRole('button', { name: 'Показать ещё' }),
  ).not.toBeInTheDocument();
});

test('shows a table on a computer', async () => {
  vi.stubGlobal('matchMedia', (query: string) => ({
    matches: true,
    media: query,
    onchange: null,
    addListener: () => {},
    removeListener: () => {},
    addEventListener: () => {},
    removeEventListener: () => {},
    dispatchEvent: () => false,
  }));
  renderSection(() => json(page([expense()])));

  const table = await screen.findByRole('table', { name: 'Расходы за месяц' });
  for (const column of [
    'Дата',
    'Категория',
    'Кто',
    'Сумма',
    'Оплата',
    'Комментарий',
  ])
    expect(
      within(table).getByRole('columnheader', { name: column }),
    ).toBeInTheDocument();
  expect(within(table).getByText('Общее')).toBeInTheDocument();
});

const dialogClosed = () =>
  waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument());

const created = () => json({ id: crypto.randomUUID() }, 201);
const noContent = () => Promise.resolve(new Response(null, { status: 204 }));

test('adds an expense with an Idempotency-Key and keeps the key on a retry', async () => {
  let failFirst = true;
  const { writes, expenseUrls } = renderSection((_, sent) => {
    if (sent.method === 'POST') {
      if (failFirst) {
        failFirst = false;
        return json({ title: 'Ошибка сервера' }, 500);
      }
      return created();
    }
    return json(page([expense()]));
  });
  await screen.findByRole('list', { name: 'Расходы за месяц' });

  await userEvent.click(
    screen.getByRole('button', { name: 'Добавить расход' }),
  );
  const dialog = await screen.findByRole('dialog', { name: 'Новый расход' });
  expect(within(dialog).getByLabelText(/Дата/)).toHaveValue('2026-10-01');

  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );
  expect(within(dialog).getByText('Выберите категорию')).toBeInTheDocument();
  expect(within(dialog).getByText('Введите сумму')).toBeInTheDocument();
  expect(writes()).toHaveLength(0);

  await userEvent.click(
    within(dialog).getByRole('combobox', { name: /Категория/ }),
  );
  await userEvent.click(await screen.findByRole('option', { name: 'Дом' }));
  await userEvent.type(within(dialog).getByLabelText(/Сумма/), '250,5');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );

  expect(await within(dialog).findByRole('alert')).toHaveTextContent(
    'Не удалось сохранить',
  );
  const listLoads = expenseUrls().length;
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );
  await dialogClosed();

  const [first, retry] = writes();
  expect(first!.url.pathname).toBe('/api/expenses');
  expect(first!.body).toEqual({
    date: '2026-10-01',
    payer: 'Shared',
    categoryId: 'c-home',
    amount: 250.5,
    paymentMethod: 'Card',
    comment: null,
  });
  const key = first!.headers.get('Idempotency-Key');
  expect(key).toBeTruthy();
  expect(retry!.headers.get('Idempotency-Key')).toBe(key);
  await waitFor(() => expect(expenseUrls().length).toBeGreaterThan(listLoads));
});

test('edits an expense with its version in If-Match', async () => {
  const item = expense({ version: 4 });
  const { writes } = renderSection((_, sent) =>
    sent.method === 'PUT' ? noContent() : json(page([item])),
  );
  await screen.findByRole('list', { name: 'Расходы за месяц' });

  await userEvent.click(
    screen.getByRole('button', { name: /Действия: Продукты/ }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Изменить' }));
  const dialog = await screen.findByRole('dialog', {
    name: 'Изменить расход',
  });
  const amount = within(dialog).getByLabelText(/Сумма/);
  expect(amount).toHaveValue('1234,50');
  await userEvent.clear(amount);
  await userEvent.type(amount, '99');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );
  await dialogClosed();

  const [put] = writes();
  expect(put!.method).toBe('PUT');
  expect(put!.url.pathname).toBe(`/api/expenses/${item.id}`);
  expect(put!.headers.get('If-Match')).toBe('4');
  expect(put!.headers.has('Idempotency-Key')).toBe(false);
  expect(put!.body).toMatchObject({ amount: 99, comment: 'Сильпо' });
});

test('says when the expense was changed elsewhere and reloads the list', async () => {
  const { expenseUrls } = renderSection((_, sent) =>
    sent.method === 'PUT'
      ? json(
          {
            title: 'Conflict',
            detail: 'The entry was changed meanwhile.',
            code: 'expenses.changed_meanwhile',
          },
          409,
        )
      : json(page([expense()])),
  );
  await screen.findByRole('list', { name: 'Расходы за месяц' });

  await userEvent.click(
    screen.getByRole('button', { name: /Действия: Продукты/ }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Изменить' }));
  const dialog = await screen.findByRole('dialog', {
    name: 'Изменить расход',
  });
  const loads = expenseUrls().length;
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Сохранить' }),
  );

  expect(await within(dialog).findByRole('alert')).toHaveTextContent(
    'Запись изменили в другом месте',
  );
  await waitFor(() => expect(expenseUrls().length).toBeGreaterThan(loads));
});

test('deletes an expense only after the confirmation', async () => {
  const item = expense();
  const { writes } = renderSection((_, sent) =>
    sent.method === 'DELETE' ? noContent() : json(page([item])),
  );
  await screen.findByRole('list', { name: 'Расходы за месяц' });

  await userEvent.click(
    screen.getByRole('button', { name: /Действия: Продукты/ }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Удалить' }));
  const dialog = await screen.findByRole('dialog', {
    name: 'Удалить расход?',
  });
  expect(writes()).toHaveLength(0);
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Удалить' }),
  );
  await dialogClosed();

  expect(writes()).toHaveLength(1);
  expect(writes()[0]!.method).toBe('DELETE');
  expect(writes()[0]!.url.pathname).toBe(`/api/expenses/${item.id}`);
});
