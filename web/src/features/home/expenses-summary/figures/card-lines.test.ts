import type { MonthSummary } from '../data/summary-api';
import {
  expensesCaption,
  incomesCaption,
  savingsCaption,
  savingsGoalPercent,
  topCategoryLines,
  whoSpentLines,
} from './card-lines';

const plain = (text: string) => text.replace(/\s/g, ' ');

const summary = (overrides: Partial<MonthSummary> = {}): MonthSummary => ({
  year: 2026,
  month: 11,
  incomes: 50000,
  incomesByPerson: [{ person: 'Maria', amount: 20000 }],
  expenses: 30000,
  expensesByPayer: [
    { payer: 'Vitaliy', amount: 10000 },
    { payer: 'Shared', amount: 20000 },
  ],
  monthlyLimit: 0,
  daysLeft: 5,
  result: 10000,
  savingsNet: 10000,
  savingsBalance: 99_600,
  savingsGoal: 100_000,
  limits: [],
  topCategories: [],
  outsideLimitsCount: 0,
  outsideLimitsAmount: 0,
  carriedIn: 0,
  carriesOut: 0,
  forecast: {
    planned: 0,
    expectedExpenses: 30000,
    expectedResult: 10000,
    expectedTotalLeft: 0,
  },
  ...overrides,
});

test('names what each of the two earned, 0 for nothing', () => {
  expect(plain(incomesCaption(summary()))).toBe('Виталий 0 ₴ · Маша 20 000 ₴');
});

test('says when there is no monthly limit', () => {
  expect(expensesCaption(summary())).toBe('лимит не задан');
});

test('the jar card rounds the goal percent down, like «План копилки»', () => {
  expect(savingsGoalPercent(summary())).toBe(99);
  expect(plain(savingsCaption(summary()))).toBe('99% от цели 100 000 ₴');
  expect(savingsGoalPercent(summary({ savingsGoal: 0 }))).toBeNull();
  expect(savingsCaption(summary({ savingsGoal: 0 }))).toBe('цель не задана');
});

test('who spent what lists every payer with a share, and nothing without expenses', () => {
  expect(
    whoSpentLines(summary()).map((line) => [line.name, line.caption]),
  ).toEqual([
    ['Виталий', '33% расходов'],
    ['Маша', '0% расходов'],
    ['Общее', '67% расходов'],
  ]);
  expect(whoSpentLines(summary({ expenses: 0 }))).toEqual([]);
});

test('keeps the five biggest categories', () => {
  const topCategories = [1, 2, 3, 4, 5, 6].map((n) => ({
    categoryId: `c${n}`,
    name: `К${n}`,
    amount: 100 - n,
  }));
  expect(
    topCategoryLines(summary({ topCategories })).map((l) => l.key),
  ).toEqual(['c1', 'c2', 'c3', 'c4', 'c5']);
});
