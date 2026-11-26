import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import { createQueryClient } from '../../../platform/query-client';
import { HomePage } from './HomePage';

afterEach(() => vi.unstubAllGlobals());

const summary = {
  year: 2026,
  month: 10,
  incomes: 50000,
  incomesByPerson: [
    { person: 'Vitaliy', amount: 30000 },
    { person: 'Maria', amount: 20000 },
  ],
  expenses: 36000,
  expensesByPayer: [
    { payer: 'Vitaliy', amount: 6000 },
    { payer: 'Maria', amount: 12000 },
    { payer: 'Shared', amount: 18000 },
  ],
  monthlyLimit: 40000,
  daysLeft: 4,
  result: -1500,
  savingsNet: 15500,
  savingsBalance: 60000,
  savingsGoal: 100000,
  limits: [
    {
      id: 'l1',
      name: 'Продукты',
      period: 'Month',
      from: '2026-10-01',
      to: '2026-10-31',
      amount: 15000,
      spent: 16500,
      left: -1500,
      percent: 110,
      perDay: 0,
    },
    {
      id: 'l2',
      name: 'Одежда',
      period: 'Quarter',
      from: '2026-10-01',
      to: '2026-12-31',
      amount: 9000,
      spent: 3000,
      left: 6000,
      percent: 33,
      perDay: null,
    },
  ],
  topCategories: [
    { categoryId: 'c1', name: 'Продукты', amount: 16500 },
    { categoryId: 'c2', name: 'Кафе', amount: 7000 },
  ],
};

const json = (body: unknown, status = 200) =>
  Promise.resolve(
    new Response(JSON.stringify(body), {
      status,
      headers: { 'Content-Type': 'application/json' },
    }),
  );

function renderHome(path = '/?year=2026&month=10', status = 200) {
  const urls: URL[] = [];
  vi.stubGlobal('fetch', (request: Request) => {
    const url = new URL(request.url);
    urls.push(url);
    return status === 200
      ? json({
          ...summary,
          month: Number(url.searchParams.get('month')),
        })
      : json({ title: 'Сервер недоступен', status }, status);
  });
  const client = createQueryClient();
  client.setDefaultOptions({ queries: { retry: false } });
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={[path]}>
          <HomePage />
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
  return { urls };
}

const plain = (element: HTMLElement) =>
  element.textContent?.replace(/\s/g, ' ');

test('opens the expenses summary in the switcher, other sections are coming', async () => {
  renderHome('/');

  const switcher = screen.getByRole('group', { name: 'Сводка' });
  expect(
    within(switcher).getByRole('button', { name: /Расходы/ }),
  ).toHaveAttribute('aria-pressed', 'true');
  expect(within(switcher).getByRole('button', { name: /Дела/ })).toBeDisabled();
  expect(screen.getByRole('link', { name: /Состояние/ })).toHaveAttribute(
    'href',
    '/status',
  );
  await screen.findByRole('region', { name: 'Доходы' });
});

test('shows the month figures, the limits and who spent what', async () => {
  renderHome();

  const incomes = await screen.findByRole('region', { name: 'Доходы' });
  expect(plain(incomes)).toContain('50 000 ₴');
  expect(plain(incomes)).toContain('Виталий 30 000 ₴ · Маша 20 000 ₴');
  expect(plain(screen.getByRole('region', { name: 'Расходы' }))).toContain(
    'лимит 40 000 ₴ · 90%',
  );
  const result = screen.getByRole('region', { name: 'Итог месяца' });
  expect(within(result).getByText(/^−1\s500/)).toHaveStyle({
    color: theme.colorSchemes.light!.palette.error.main,
  });
  expect(plain(result)).toContain('отложено 15 500 ₴');
  expect(plain(screen.getByRole('region', { name: 'Копилка' }))).toContain(
    '60% от цели 100 000 ₴',
  );

  const rows = within(
    screen.getByRole('list', { name: 'Лимиты месяца' }),
  ).getAllByRole('listitem');
  expect(rows).toHaveLength(3);
  expect(plain(rows[0]!)).toContain('Продукты110%');
  expect(within(rows[0]!).getByText(/остаток −1\s500,00/)).toHaveStyle({
    color: theme.colorSchemes.light!.palette.error.main,
  });
  expect(rows[1]).toHaveTextContent('Одежда (квартал)');
  expect(rows[1]).not.toHaveTextContent('в день');
  expect(plain(rows[2]!)).toContain('ИТОГО90%');
  expect(plain(rows[2]!)).toContain('остаток 4 000,00 ₴ · в день 1 000,00 ₴');

  const who = screen.getByRole('list', { name: 'Кто сколько потратил' });
  expect(plain(who)).toContain('Общее50% расходов18 000,00 ₴');
  const top = screen.getByRole('list', { name: 'Топ-5 категорий' });
  expect(within(top).getAllByRole('listitem')).toHaveLength(2);
});

test('loads the next month from the arrows', async () => {
  const { urls } = renderHome();
  await screen.findByRole('region', { name: 'Доходы' });

  await userEvent.click(
    screen.getByRole('button', { name: 'Следующий месяц' }),
  );

  await waitFor(() => expect(urls).toHaveLength(2));
  expect(urls[1]!.pathname).toBe('/api/expenses/summary');
  expect(urls[1]!.searchParams.get('year')).toBe('2026');
  expect(urls[1]!.searchParams.get('month')).toBe('11');
  expect(screen.getByText('Ноябрь 2026')).toBeInTheDocument();
});

test('says why the summary did not load and offers to retry', async () => {
  renderHome('/', 500);

  expect(await screen.findByRole('alert')).toHaveTextContent(
    'Не удалось загрузить сводку',
  );
  expect(screen.getByRole('button', { name: 'Повторить' })).toBeInTheDocument();
});
