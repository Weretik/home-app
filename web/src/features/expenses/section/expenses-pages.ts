/**
 * The pages of the Expenses section, shown as tabs (the sheets of the old Google table). A page is added here and in
 * the routes (src/app/root/App.tsx) when it is built; planned ones (Обзор, Лимиты, План, Платежи, По месяцам, Копилка,
 * Категории) are not listed until then, so there are no dead tabs.
 */
export interface ExpensesPage {
  /** Path under /expenses; '' is the index page. */
  path: string;
  label: string;
}

export const expensesPages: ExpensesPage[] = [
  { path: '', label: 'Расходы' },
  { path: 'incomes', label: 'Доходы' },
];

export const sectionPath = '/expenses';

export const pageUrl = (page: ExpensesPage) =>
  page.path ? `${sectionPath}/${page.path}` : sectionPath;
