import { fillOf, limitRows } from './limit-rows';
import type { MonthSummary } from './summary-api';

const summary = (overrides: Partial<MonthSummary> = {}): MonthSummary => ({
  year: 2026,
  month: 11,
  incomes: 0,
  incomesByPerson: [],
  expenses: 9000,
  expensesByPayer: [],
  monthlyLimit: 12000,
  daysLeft: 5,
  result: 0,
  savingsNet: 0,
  savingsBalance: 0,
  savingsGoal: 0,
  limits: [],
  topCategories: [],
  ...overrides,
});

test('adds the ИТОГО row from all expenses against the monthly limit', () => {
  expect(limitRows(summary()).at(-1)).toEqual({
    key: 'total',
    name: 'ИТОГО',
    spent: 9000,
    limit: 12000,
    left: 3000,
    percent: 75,
    perDay: 600,
    isTotal: true,
  });
});

test('leaves the comparison out without a monthly limit or outside the month', () => {
  const total = limitRows(summary({ monthlyLimit: 0 })).at(-1);
  expect(total).toMatchObject({ limit: null, left: null, percent: null });
  expect(limitRows(summary({ daysLeft: null })).at(-1)?.perDay).toBeNull();
  expect(limitRows(summary({ expenses: 15000 })).at(-1)?.perDay).toBe(0);
});

test('labels quarterly limits and colours the fill by how much is spent', () => {
  const [row] = limitRows(
    summary({
      limits: [
        {
          id: 'a',
          name: 'Одежда',
          period: 'Quarter',
          from: '2026-10-01',
          to: '2026-12-31',
          amount: 6000,
          spent: 1000,
          left: 5000,
          percent: 17,
          perDay: null,
        },
      ],
    }),
  );
  expect(row?.name).toBe('Одежда (квартал)');
  expect([fillOf(79), fillOf(80), fillOf(100), fillOf(101)]).toEqual([
    'success',
    'warning',
    'warning',
    'error',
  ]);
});
