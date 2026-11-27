import { payerLabels, personLabels } from '../data/labels';
import { formatWhole, goalPercentOf, percentOf } from '../data/money';
import type { MonthSummary, Payer, Person } from '../data/summary-api';
import { totalPercent } from '../limits/limit-rows';

const payers: Payer[] = ['Vitaliy', 'Maria', 'Shared'];

/** What `person` earned in the month; 0 without incomes. */
export const earnedBy = (summary: MonthSummary, person: Person) =>
  summary.incomesByPerson.find((i) => i.person === person)?.amount ?? 0;

/** «Віталій 30 000 ₴ · Маша 20 000 ₴» under «Доходи». */
export const incomesCaption = (summary: MonthSummary) =>
  (['Vitaliy', 'Maria'] as const)
    .map(
      (person) =>
        `${personLabels[person]} ${formatWhole(earnedBy(summary, person))}`,
    )
    .join(' · ');

/** «ліміт 40 000 ₴ · 90%» under «Витрати» (the «РАЗОМ» percent), or that there is no limit. */
export const expensesCaption = (summary: MonthSummary) =>
  summary.monthlyLimit > 0
    ? `ліміт ${formatWhole(summary.monthlyLimit)} · ${totalPercent(summary)}%`
    : 'ліміт не задано';

/** The share of the savings goal for the «Скарбничка» card; null without a goal (no bar). */
export const savingsGoalPercent = (summary: MonthSummary) =>
  summary.savingsGoal > 0
    ? goalPercentOf(summary.savingsBalance, summary.savingsGoal)
    : null;

/** «60% від цілі 100 000 ₴» under «Скарбничка», or that there is no goal. */
export const savingsCaption = (summary: MonthSummary) => {
  const percent = savingsGoalPercent(summary);
  return percent === null
    ? 'ціль не задано'
    : `${percent}% від цілі ${formatWhole(summary.savingsGoal)}`;
};

/** One name / amount line of a small card. */
export interface AmountLine {
  key: string;
  name: string;
  amount: number;
  caption?: string;
}

/** «Хто скільки витратив»: each payer with the share of the month's expenses; none in a month without expenses. */
export function whoSpentLines(summary: MonthSummary): AmountLine[] {
  if (summary.expenses <= 0) return [];
  const spentBy = (payer: Payer) =>
    summary.expensesByPayer.find((p) => p.payer === payer)?.amount ?? 0;
  return payers.map((payer) => ({
    key: payer,
    name: payerLabels[payer],
    amount: spentBy(payer),
    caption: `${percentOf(spentBy(payer), summary.expenses)}% витрат`,
  }));
}

/** «Топ-5 категорій»: the five biggest categories of the month. */
export const topCategoryLines = (summary: MonthSummary): AmountLine[] =>
  summary.topCategories.slice(0, 5).map((category) => ({
    key: category.categoryId,
    name: category.name,
    amount: category.amount,
  }));
