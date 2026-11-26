import { payerLabels, personLabels } from '../data/labels';
import { formatWhole, goalPercentOf, percentOf } from '../data/money';
import type { MonthSummary, Payer, Person } from '../data/summary-api';
import { totalPercent } from '../limits/limit-rows';

const payers: Payer[] = ['Vitaliy', 'Maria', 'Shared'];

/** What `person` earned in the month; 0 without incomes. */
export const earnedBy = (summary: MonthSummary, person: Person) =>
  summary.incomesByPerson.find((i) => i.person === person)?.amount ?? 0;

/** «Виталий 30 000 ₴ · Маша 20 000 ₴» under «Доходы». */
export const incomesCaption = (summary: MonthSummary) =>
  (['Vitaliy', 'Maria'] as const)
    .map(
      (person) =>
        `${personLabels[person]} ${formatWhole(earnedBy(summary, person))}`,
    )
    .join(' · ');

/** «лимит 40 000 ₴ · 90%» under «Расходы» (the «ИТОГО» percent), or that there is no limit. */
export const expensesCaption = (summary: MonthSummary) =>
  summary.monthlyLimit > 0
    ? `лимит ${formatWhole(summary.monthlyLimit)} · ${totalPercent(summary)}%`
    : 'лимит не задан';

/** The share of the savings goal for the «Копилка» card; null without a goal (no bar). */
export const savingsGoalPercent = (summary: MonthSummary) =>
  summary.savingsGoal > 0
    ? goalPercentOf(summary.savingsBalance, summary.savingsGoal)
    : null;

/** «60% от цели 100 000 ₴» under «Копилка», or that there is no goal. */
export const savingsCaption = (summary: MonthSummary) => {
  const percent = savingsGoalPercent(summary);
  return percent === null
    ? 'цель не задана'
    : `${percent}% от цели ${formatWhole(summary.savingsGoal)}`;
};

/** One name / amount line of a small card. */
export interface AmountLine {
  key: string;
  name: string;
  amount: number;
  caption?: string;
}

/** «Кто сколько потратил»: each payer with the share of the month's expenses; none in a month without expenses. */
export function whoSpentLines(summary: MonthSummary): AmountLine[] {
  if (summary.expenses <= 0) return [];
  const spentBy = (payer: Payer) =>
    summary.expensesByPayer.find((p) => p.payer === payer)?.amount ?? 0;
  return payers.map((payer) => ({
    key: payer,
    name: payerLabels[payer],
    amount: spentBy(payer),
    caption: `${percentOf(spentBy(payer), summary.expenses)}% расходов`,
  }));
}

/** «Топ-5 категорий»: the five biggest categories of the month. */
export const topCategoryLines = (summary: MonthSummary): AmountLine[] =>
  summary.topCategories.slice(0, 5).map((category) => ({
    key: category.categoryId,
    name: category.name,
    amount: category.amount,
  }));
