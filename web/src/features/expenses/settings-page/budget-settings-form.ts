import type { BudgetView } from '../budget-data/budget-api';
import type { BudgetSettingsRequest } from '../budget-data/budget-writes-api';
import {
  MAX_AMOUNT,
  amountText,
  parseAmount,
  type FieldErrors,
} from '../entry-editing/entry-fields';

/** The monthly limit and the savings goal as typed; empty means none (0). */
export interface BudgetSettingsValues {
  monthlyLimit: string;
  savingsGoal: string;
}

const shown = (value: number) => (value > 0 ? amountText(value) : '');

export function budgetSettingsValues(budget: BudgetView): BudgetSettingsValues {
  return {
    monthlyLimit: shown(budget.monthlyLimit),
    savingsGoal: shown(budget.savingsGoal),
  };
}

/** An amount that may be left empty (none); else the same rules as any amount, zero allowed. */
export function optionalAmountError(text: string): string | undefined {
  if (text.trim() === '') return undefined;
  const value = parseAmount(text);
  if (value === undefined)
    return 'Сумма — число, не больше двух знаков после запятой';
  if (value > MAX_AMOUNT) return 'Слишком большая сумма';
  return undefined;
}

export function validateBudgetSettings(
  values: BudgetSettingsValues,
): FieldErrors {
  return {
    monthlyLimit: optionalAmountError(values.monthlyLimit),
    savingsGoal: optionalAmountError(values.savingsGoal),
  };
}

export function toBudgetSettingsRequest(
  values: BudgetSettingsValues,
): BudgetSettingsRequest {
  return {
    monthlyLimit: parseAmount(values.monthlyLimit) ?? 0,
    savingsGoal: parseAmount(values.savingsGoal) ?? 0,
  };
}
