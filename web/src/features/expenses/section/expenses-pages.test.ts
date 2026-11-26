import { expensesPages, pageUrl, settingsPath } from './expenses-pages';

test('the index page is the section itself, the others are under it', () => {
  expect(expensesPages.map(pageUrl)).toEqual([
    '/expenses',
    '/expenses/incomes',
    '/expenses/savings',
    '/expenses/forecast',
  ]);
  expect(settingsPath).toBe('/expenses/settings');
});
