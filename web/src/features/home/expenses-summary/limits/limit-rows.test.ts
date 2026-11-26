import { fillOf, limitNotes, limitRows } from './limit-rows';
import type { MonthSummary } from '../data/summary-api';

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
  outsideLimitsCount: 0,
  outsideLimitsAmount: 0,
  carriedIn: 0,
  carriesOut: 0,
  forecast: {
    planned: 0,
    expectedExpenses: 9000,
    expectedResult: 0,
    expectedTotalLeft: 3000,
  },
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
    carriedIn: 0,
    carriesOut: 0,
    planned: 0,
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
          carriedIn: 0,
          carriesOut: 0,
          planned: 0,
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

test('ИТОГО leaves out expenses outside the limits and counts the overspend carried in', () => {
  const total = limitRows(
    summary({
      expenses: 9000,
      outsideLimitsCount: 1,
      outsideLimitsAmount: 2000,
      carriedIn: 1500,
      carriesOut: 0,
      forecast: {
        planned: 700,
        expectedExpenses: 9700,
        expectedResult: 0,
        expectedTotalLeft: 2800,
      },
    }),
  ).at(-1);
  expect(total).toMatchObject({
    spent: 7000,
    left: 3500,
    percent: 71,
    perDay: 700,
    carriedIn: 1500,
    planned: 700,
  });
});

test('passes on what each limit carries and plans', () => {
  const [row] = limitRows(
    summary({
      limits: [
        {
          id: 'a',
          name: 'Кафе',
          period: 'Month',
          from: '2026-11-01',
          to: '2026-11-30',
          amount: 3000,
          spent: 2800,
          left: -300,
          percent: 110,
          perDay: 0,
          carriedIn: 500,
          carriesOut: 300,
          planned: 400,
        },
      ],
    }),
  );
  expect(row).toMatchObject({ carriedIn: 500, carriesOut: 300, planned: 400 });
});

test('notes only the carry-over and plans that are there', () => {
  expect(limitNotes({ carriedIn: 0, carriesOut: 0, planned: 0 })).toEqual([]);
  expect(
    limitNotes({ carriedIn: 1200, carriesOut: 300.5, planned: 500 }).map(
      (note) => note.text.replace(/\s/g, ' '),
    ),
  ).toEqual(['перенос 1 200,00 ₴', '→ след. мес. 300,50 ₴', 'план 500,00 ₴']);
  expect(
    limitNotes({ carriedIn: 0, carriesOut: 0, planned: 250 }).map(
      (note) => note.key,
    ),
  ).toEqual(['planned']);
});
