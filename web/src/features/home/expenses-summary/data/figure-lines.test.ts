import {
  forecastLines,
  formatSpendingCount,
  isEmptyMonth,
  outsideLimitsLine,
} from './figure-lines';
import type { MonthSummary } from './summary-api';

const plain = (text: string | null) => text?.replace(/\s/g, ' ');

const summary = (overrides: Partial<MonthSummary> = {}): MonthSummary => ({
  year: 2026,
  month: 11,
  incomes: 50000,
  incomesByPerson: [],
  expenses: 30000,
  expensesByPayer: [],
  monthlyLimit: 40000,
  daysLeft: 5,
  result: 10000,
  savingsNet: 10000,
  savingsBalance: 0,
  savingsGoal: 0,
  limits: [],
  topCategories: [],
  outsideLimitsCount: 0,
  outsideLimitsAmount: 0,
  carriedIn: 0,
  carriesOut: 0,
  forecast: {
    planned: 5000,
    expectedExpenses: 35000,
    expectedResult: 5000,
    expectedTotalLeft: 5000,
  },
  ...overrides,
});

test('counts spendings in Russian', () => {
  expect([1, 3, 5, 21].map(formatSpendingCount)).toEqual([
    '1 трата',
    '3 траты',
    '5 трат',
    '21 трата',
  ]);
});

test('shows the outside-limits line only when there are such expenses', () => {
  expect(outsideLimitsLine(summary())).toBeNull();
  expect(
    plain(
      outsideLimitsLine(
        summary({ outsideLimitsCount: 2, outsideLimitsAmount: 3500.4 }),
      ),
    ),
  ).toBe('Вне лимитов: 2 траты на 3 500 ₴');
});

test('forecasts the month, with what stays of the limit only when there is one', () => {
  const lines = forecastLines(summary());
  expect(lines.map((line) => [line.name, line.amount, line.tone])).toEqual([
    ['Ожидаемые расходы', 35000, undefined],
    ['Запланировано', 5000, undefined],
    ['Ожидаемый итог месяца', 5000, 'success'],
    ['Останется от лимита', 5000, undefined],
  ]);
  const over = forecastLines(
    summary({
      monthlyLimit: 0,
      forecast: {
        planned: 0,
        expectedExpenses: 30000,
        expectedResult: -200,
        expectedTotalLeft: 0,
      },
    }),
  );
  expect(over.map((line) => line.key)).toEqual([
    'expected',
    'planned',
    'result',
  ]);
  expect(over.at(-1)?.tone).toBe('error');
});

test('a month is empty only without incomes, expenses, jar movements and plans', () => {
  const nothing = {
    incomes: 0,
    expenses: 0,
    savingsNet: 0,
    forecast: { ...summary().forecast, planned: 0 },
  };
  expect(isEmptyMonth(summary(nothing))).toBe(true);
  expect(isEmptyMonth(summary({ ...nothing, savingsNet: -100 }))).toBe(false);
  expect(
    isEmptyMonth(summary({ ...nothing, forecast: summary().forecast })),
  ).toBe(false);
});
