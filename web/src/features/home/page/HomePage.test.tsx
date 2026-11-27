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
      carriedIn: 0,
      carriesOut: 1500,
      planned: 0,
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
      carriedIn: 0,
      carriesOut: 0,
      planned: 0,
    },
  ],
  topCategories: [
    { categoryId: 'c1', name: 'Продукты', amount: 16500 },
    { categoryId: 'c2', name: 'Кафе', amount: 7000 },
  ],
  outsideLimitsCount: 0,
  outsideLimitsAmount: 0,
  carriedIn: 0,
  carriesOut: 0,
  forecast: {
    planned: 2000,
    expectedExpenses: 38000,
    expectedResult: -3500,
    expectedTotalLeft: 2000,
  },
};

const json = (body: unknown, status = 200) =>
  Promise.resolve(
    new Response(JSON.stringify(body), {
      status,
      headers: { 'Content-Type': 'application/json' },
    }),
  );

function renderHome(
  path = '/?year=2026&month=10',
  status = 200,
  overrides: Record<string, unknown> = {},
) {
  const urls: URL[] = [];
  vi.stubGlobal('fetch', (request: Request) => {
    const url = new URL(request.url);
    urls.push(url);
    return status === 200
      ? json({
          ...summary,
          ...overrides,
          month: Number(url.searchParams.get('month')),
        })
      : json({ title: 'Сервер недоступний', status }, status);
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

test('opens the budget summary first, every section has its summary', async () => {
  renderHome('/');

  const switcher = screen.getByRole('group', { name: 'Зведення' });
  expect(
    within(switcher).getByRole('button', { name: /Бюджет/ }),
  ).toHaveAttribute('aria-pressed', 'true');
  expect(
    within(switcher).getByRole('button', { name: /Подарунки/ }),
  ).toBeEnabled();
  expect(
    within(switcher).getByRole('button', { name: /Справи/ }),
  ).toBeEnabled();
  expect(
    within(switcher).getByRole('button', { name: /Покупки/ }),
  ).toBeEnabled();
  expect(screen.getByRole('link', { name: /Стан/ })).toHaveAttribute(
    'href',
    '/status',
  );
  await screen.findByRole('region', { name: 'Доходи' });
});

test('shows the month figures, the limits and who spent what', async () => {
  renderHome();

  const incomes = await screen.findByRole('region', { name: 'Доходи' });
  expect(plain(incomes)).toContain('50 000 ₴');
  expect(plain(incomes)).toContain('Віталій 30 000 ₴ · Маша 20 000 ₴');
  expect(plain(screen.getByRole('region', { name: 'Витрати' }))).toContain(
    'ліміт 40 000 ₴ · 90%',
  );
  const result = screen.getByRole('region', { name: 'Підсумок місяця' });
  expect(within(result).getByText(/^−1\s500/)).toHaveStyle({
    color: theme.colorSchemes.light!.palette.error.main,
  });
  expect(plain(result)).toContain('відкладено 15 500 ₴');
  expect(plain(screen.getByRole('region', { name: 'Скарбничка' }))).toContain(
    '60% від цілі 100 000 ₴',
  );

  const rows = within(
    screen.getByRole('list', { name: 'Ліміти місяця' }),
  ).getAllByRole('listitem');
  expect(rows).toHaveLength(3);
  expect(plain(rows[0]!)).toContain('Продукты110%');
  expect(within(rows[0]!).getByText(/залишок −1\s500,00/)).toHaveStyle({
    color: theme.colorSchemes.light!.palette.error.main,
  });
  expect(plain(rows[0]!)).toContain('→ наст. міс. 1 500,00 ₴');
  expect(rows[0]).not.toHaveTextContent('перенесення');
  expect(rows[1]).toHaveTextContent('Одежда (квартал)');
  expect(rows[1]).not.toHaveTextContent('наст. міс.');
  expect(rows[1]).not.toHaveTextContent('на день');
  expect(plain(rows[2]!)).toContain('РАЗОМ90%');
  expect(plain(rows[2]!)).toContain('залишок 4 000,00 ₴ · на день 1 000,00 ₴');

  expect(screen.getByRole('region', { name: 'Витрати' })).not.toHaveTextContent(
    'Поза лімітами',
  );
  const forecast = screen.getByRole('region', { name: 'Прогноз' });
  expect(plain(forecast)).toContain('Очікувані витрати38 000,00 ₴');
  expect(plain(forecast)).toContain('Заплановано2 000,00 ₴');
  expect(plain(forecast)).toContain('Очікуваний підсумок місяця−3 500,00 ₴');
  expect(plain(forecast)).toContain('Залишиться від ліміту2 000,00 ₴');
  expect(
    within(forecast).getByRole('link', { name: 'Відкрити план' }),
  ).toHaveAttribute('href', '/expenses/forecast?year=2026&month=10');

  const who = screen.getByRole('list', { name: 'Хто скільки витратив' });
  expect(plain(who)).toContain('Спільне50% витрат18 000,00 ₴');
  const top = screen.getByRole('list', { name: 'Топ-5 категорій' });
  expect(within(top).getAllByRole('listitem')).toHaveLength(2);
});

test('counts the carry-over and the expenses outside the limits in РАЗОМ', async () => {
  renderHome('/?year=2026&month=10', 200, {
    outsideLimitsCount: 2,
    outsideLimitsAmount: 4000,
    carriedIn: 3000,
    carriesOut: 0,
  });

  const expenses = await screen.findByRole('region', { name: 'Витрати' });
  expect(plain(expenses)).toContain('Поза лімітами: 2 витрати на 4 000 ₴');
  // (36 000 − 4 000 outside the limits + 3 000 carried in) of 40 000.
  expect(plain(expenses)).toContain('ліміт 40 000 ₴ · 88%');
  const total = within(
    screen.getByRole('list', { name: 'Ліміти місяця' }),
  ).getAllByRole('listitem')[2]!;
  expect(plain(total)).toContain('РАЗОМ88%');
  expect(plain(total)).toContain('32 000,00 ₴ з 40 000,00 ₴');
  expect(plain(total)).toContain('залишок 5 000,00 ₴');
  expect(plain(total)).toContain('перенесення 3 000,00 ₴ · план 2 000,00 ₴');
});

test('loads the next month from the arrows', async () => {
  const { urls } = renderHome();
  await screen.findByRole('region', { name: 'Доходи' });

  await userEvent.click(
    screen.getByRole('button', { name: 'Наступний місяць' }),
  );

  await waitFor(() => expect(urls).toHaveLength(2));
  expect(urls[1]!.pathname).toBe('/api/expenses/summary');
  expect(urls[1]!.searchParams.get('year')).toBe('2026');
  expect(urls[1]!.searchParams.get('month')).toBe('11');
  expect(screen.getByText('Листопад 2026')).toBeInTheDocument();
});

test('says why the summary did not load and offers to retry', async () => {
  renderHome('/', 500);

  expect(await screen.findByRole('alert')).toHaveTextContent(
    'Не вдалося завантажити зведення',
  );
  expect(screen.getByRole('button', { name: 'Повторити' })).toBeInTheDocument();
});

test('opens the gifts summary from ?s=gifts', async () => {
  const counts = {
    wantedCount: 0,
    wantedPriceSum: 0,
    wantedWithoutPrice: 0,
    highCount: 0,
    giftedCount: 0,
    giftedPriceSum: 0,
  };
  const urls: URL[] = [];
  vi.stubGlobal('fetch', (request: Request) => {
    urls.push(new URL(request.url));
    return json({
      recipients: ['Maria', 'Vitaliy', 'Together'].map((recipient) => ({
        recipient,
        counts,
        topWanted: [],
      })),
      total: counts,
      upcoming: [],
      details: [],
    });
  });
  const client = createQueryClient();
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={['/?s=gifts']}>
          <HomePage />
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );

  const switcher = screen.getByRole('group', { name: 'Зведення' });
  expect(
    within(switcher).getByRole('button', { name: /Подарунки/ }),
  ).toHaveAttribute('aria-pressed', 'true');
  expect(await screen.findByText('Бажань поки немає')).toBeInTheDocument();
  expect(urls.map((url) => url.pathname)).toEqual(['/api/gifts/summary']);
});
