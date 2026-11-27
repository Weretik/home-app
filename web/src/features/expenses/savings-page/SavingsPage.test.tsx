import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/client/query-client';
import { ExpensesSection } from '../section/ExpensesSection';
import { SavingsPage } from './SavingsPage';

afterEach(() => vi.unstubAllGlobals());

const movement = (overrides: Record<string, unknown> = {}) => ({
  id: crypto.randomUUID(),
  version: 2,
  date: '2026-10-05',
  person: 'Vitaliy',
  amount: 1500,
  direction: 'Deposit',
  comment: 'Премия',
  ...overrides,
});

const movements = (items: unknown[]) => ({
  items,
  page: 1,
  pageSize: 50,
  totalCount: items.length,
  deposits: 1500,
  withdrawals: 700,
  balanceAtStart: 10000,
  balanceAtEnd: 10800,
  currentBalance: 12345.67,
});

const overview = {
  currentBalance: 12345.67,
  months: [
    {
      year: 2026,
      month: 10,
      deposits: 1500,
      withdrawals: 700,
      balanceAtEnd: 10800,
    },
    {
      year: 2026,
      month: 9,
      deposits: 10000,
      withdrawals: 0,
      balanceAtEnd: 10000,
    },
  ],
};

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

function renderPage(items: unknown[], savingsGoal = 100_000) {
  const sent: Sent[] = [];
  vi.stubGlobal('fetch', async (request: Request) => {
    const text = await request.text();
    const entry: Sent = {
      method: request.method,
      url: new URL(request.url),
      headers: request.headers,
      body: text ? JSON.parse(text) : undefined,
    };
    sent.push(entry);
    if (entry.method === 'POST') return json({ id: crypto.randomUUID() }, 201);
    if (entry.method !== 'GET') return new Response(null, { status: 204 });
    if (entry.url.pathname === '/api/expenses/budget')
      return json({
        version: 3,
        monthlyLimit: 50_000,
        savingsGoal,
        limits: [],
      });
    return entry.url.pathname === '/api/expenses/savings/months'
      ? json(overview)
      : json(movements(items));
  });
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={['/expenses/savings?year=2026&month=10']}>
          <Routes>
            <Route path="/expenses" element={<ExpensesSection />}>
              <Route path="savings" element={<SavingsPage />} />
            </Route>
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  return { sent };
}

const plain = (element: HTMLElement) =>
  element.textContent?.replace(/\s/g, ' ');

test('shows the balance in the jar and signed, coloured movements', async () => {
  renderPage([
    movement(),
    movement({
      person: 'Maria',
      amount: -700,
      direction: 'Withdrawal',
      comment: null,
    }),
  ]);

  const rows = within(
    await screen.findByRole('list', { name: 'Операції скарбнички за місяць' }),
  ).getAllByRole('listitem');
  expect(screen.getByRole('tab', { name: 'Скарбничка' })).toHaveAttribute(
    'aria-current',
    'page',
  );
  const balance = screen.getByRole('region', { name: 'Скарбничка' });
  await waitFor(() =>
    expect(plain(balance)).toContain('У скарбничці зараз12 345,67 ₴'),
  );

  const deposit = within(rows[0]!).getByText(/^\+1\s500,00/);
  const withdrawal = within(rows[1]!).getByText(/^−700,00/);
  const { palette } = theme.colorSchemes.light!;
  expect(deposit).toHaveStyle({ color: palette.primary.main });
  expect(withdrawal).toHaveStyle({ color: palette.error.main });
  expect(rows[1]).toHaveTextContent('Маша');

  const months = within(
    screen.getByRole('list', { name: 'Скарбничка по місяцях' }),
  ).getAllByRole('button');
  expect(months[0]).toHaveTextContent('Жовтень 2026');
  expect(plain(months[0]!)).toContain('+1 500,00 ₴');
  expect(plain(months[0]!)).toContain('−700,00 ₴');
  expect(plain(months[0]!)).toContain('10 800,00 ₴');
  expect(months[0]).toHaveAttribute('aria-current', 'true');
});

test('takes money out with a minus sign and Віталій by default', async () => {
  const { sent } = renderPage([]);
  await screen.findByText('За цей місяць записів немає');

  await userEvent.click(screen.getByRole('button', { name: 'Додати' }));
  const dialog = await screen.findByRole('dialog');
  await userEvent.click(
    within(dialog).getByRole('radio', { name: 'Взяли зі скарбнички' }),
  );
  await userEvent.type(within(dialog).getByLabelText(/Сума/), '700');
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  await waitFor(() =>
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument(),
  );

  const post = sent.find((s) => s.method === 'POST')!;
  expect(post.url.pathname).toBe('/api/expenses/savings');
  expect(post.headers.get('Idempotency-Key')).toBeTruthy();
  expect(post.body).toEqual({
    date: '2026-10-01',
    person: 'Vitaliy',
    amount: -700,
    comment: null,
  });
});

test('edits a movement with If-Match and deletes after the confirmation', async () => {
  const item = movement({ version: 7 });
  const { sent } = renderPage([item]);
  await screen.findByRole('list', { name: 'Операції скарбнички за місяць' });

  await userEvent.click(screen.getByRole('button', { name: /Дії:/ }));
  await userEvent.click(screen.getByRole('menuitem', { name: 'Змінити' }));
  const dialog = await screen.findByRole('dialog', {
    name: 'Змінити операцію',
  });
  expect(
    within(dialog).getByRole('radio', { name: 'Поклали в скарбничку' }),
  ).toBeChecked();
  await userEvent.click(
    within(dialog).getByRole('button', { name: 'Зберегти' }),
  );
  await waitFor(() =>
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument(),
  );
  const put = sent.find((s) => s.method === 'PUT')!;
  expect(put.url.pathname).toBe(`/api/expenses/savings/${item.id}`);
  expect(put.headers.get('If-Match')).toBe('7');
  expect(put.body).toMatchObject({ amount: 1500 });

  await userEvent.click(screen.getByRole('button', { name: /Дії:/ }));
  await userEvent.click(screen.getByRole('menuitem', { name: 'Видалити' }));
  const confirm = await screen.findByRole('dialog', {
    name: 'Видалити операцію?',
  });
  await userEvent.click(
    within(confirm).getByRole('button', { name: 'Видалити' }),
  );
  await waitFor(() =>
    expect(sent.some((s) => s.method === 'DELETE')).toBe(true),
  );
});

test('shows how far the jar is towards its goal', async () => {
  renderPage([movement()]);

  const card = await screen.findByRole('region', { name: 'План скарбнички' });
  expect(
    await within(card).findByRole('img', { name: 'Виконано 12%' }),
  ).toBeInTheDocument();
  expect(within(card).getByText('Виконано 12%')).toBeInTheDocument();
  const figures = plain(card);
  expect(figures).toContain('Зараз12 345,67 ₴');
  expect(figures).toContain('План100 000,00 ₴');
  expect(figures).toContain('Залишилось87 654,33 ₴');
  expect(within(card).queryByText('План виконано!')).not.toBeInTheDocument();
});

test('says the goal is reached', async () => {
  renderPage([movement()], 10_000);

  const card = await screen.findByRole('region', { name: 'План скарбнички' });
  expect(await within(card).findByText('План виконано!')).toBeInTheDocument();
  expect(
    within(card).getByRole('img', { name: 'Виконано 123%' }),
  ).toBeInTheDocument();
  expect(within(card).queryByText('Залишилось')).not.toBeInTheDocument();
});

test('offers to set a goal when there is none', async () => {
  renderPage([movement()], 0);

  const card = await screen.findByRole('region', { name: 'План скарбнички' });
  expect(
    await within(card).findByRole('link', {
      name: 'Задати план у налаштуваннях',
    }),
  ).toHaveAttribute('href', '/expenses/settings');
});
