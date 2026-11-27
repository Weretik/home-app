import type { BudgetView } from '../budget-data/budget-api';
import type { BudgetSettingsRequest } from '../budget-data/budget-writes-api';
import { kyivMonthText, monthStartDay } from '../data/month';
import {
  MAX_AMOUNT,
  amountText,
  monthError,
  parseAmount,
  type FieldErrors,
} from '../entry-editing/entry-fields';

/** The monthly limit and the savings goal as typed (empty means none, 0), and the month the limit applies from. */
export interface BudgetSettingsValues {
  monthlyLimit: string;
  /** «Діє з» as «YYYY-MM»; the current month in Kyiv to start. */
  monthlyLimitFrom: string;
  savingsGoal: string;
}

const shown = (value: number) => (value > 0 ? amountText(value) : '');

export function budgetSettingsValues(
  budget: BudgetView,
  now: Date = new Date(),
): BudgetSettingsValues {
  return {
    monthlyLimit: shown(budget.monthlyLimit),
    monthlyLimitFrom: kyivMonthText(now),
    savingsGoal: shown(budget.savingsGoal),
  };
}

/** An amount that may be left empty (none); else the same rules as any amount, zero allowed. */
export function optionalAmountError(text: string): string | undefined {
  if (text.trim() === '') return undefined;
  const value = parseAmount(text);
  if (value === undefined)
    return 'Сума — число, не більше двох знаків після коми';
  if (value > MAX_AMOUNT) return 'Завелика сума';
  return undefined;
}

export function validateBudgetSettings(
  values: BudgetSettingsValues,
): FieldErrors {
  return {
    monthlyLimit: optionalAmountError(values.monthlyLimit),
    monthlyLimitFrom: monthError(values.monthlyLimitFrom),
    savingsGoal: optionalAmountError(values.savingsGoal),
  };
}

/** The monthly limit in force (`BudgetView.monthlyLimit`) and the month the form started with. */
export interface MonthlyLimitInForce {
  monthlyLimit: number;
  monthlyLimitFrom: string;
}

/**
 * The request; `monthlyLimitFrom` (the month's first day) only when the limit differs from the one in force or the
 * month was moved off the default. An explicit month replaces every later step on the server (api R-LA-002,
 * R-LA-005), so a goal-only save must leave it out (web SDD 006, R-WLA-003).
 */
export function toBudgetSettingsRequest(
  values: BudgetSettingsValues,
  current: MonthlyLimitInForce,
): BudgetSettingsRequest {
  const monthlyLimit = parseAmount(values.monthlyLimit) ?? 0;
  const sendsMonth =
    monthlyLimit !== current.monthlyLimit ||
    values.monthlyLimitFrom !== current.monthlyLimitFrom;
  return {
    monthlyLimit,
    savingsGoal: parseAmount(values.savingsGoal) ?? 0,
    ...(sendsMonth && {
      monthlyLimitFrom: monthStartDay(values.monthlyLimitFrom),
    }),
  };
}
