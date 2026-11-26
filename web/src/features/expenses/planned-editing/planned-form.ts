import type { Payer } from '../data/expenses-api';
import type { Month } from '../data/month';
import {
  amountError,
  amountText,
  parseAmount,
  type FieldErrors,
} from '../entry-editing/entry-fields';
import type { PlannedExpenseItem } from '../planned-data/planned-api';
import type { PlannedExpenseRequest } from '../planned-data/planned-writes-api';

/** The API's limit on a plan's name (PlannedExpense.MaxNameLength). */
export const MAX_PLAN_NAME = 100;

/** The planned expense form as typed; `year` and `month` are the plan's month, not edited in the form. */
export interface PlannedFormValues {
  year: number;
  month: number;
  name: string;
  categoryId: string;
  payer: Payer;
  amount: string;
}

/** The form of an existing plan, or a new shared one in the open month. */
export function plannedFormValues(
  item: PlannedExpenseItem | undefined,
  month: Month,
): PlannedFormValues {
  return item
    ? {
        year: item.year,
        month: item.month,
        name: item.name,
        categoryId: item.categoryId,
        payer: item.payer,
        amount: amountText(item.amount),
      }
    : {
        year: month.year,
        month: month.month,
        name: '',
        categoryId: '',
        payer: 'Shared',
        amount: '',
      };
}

export function validatePlanned(values: PlannedFormValues): FieldErrors {
  const name = values.name.trim();
  return {
    name: !name
      ? 'Введите название'
      : name.length > MAX_PLAN_NAME
        ? `Не длиннее ${MAX_PLAN_NAME} символов`
        : undefined,
    categoryId: values.categoryId ? undefined : 'Выберите категорию',
    amount: amountError(values.amount),
  };
}

export function toPlannedRequest(
  values: PlannedFormValues,
): PlannedExpenseRequest {
  return {
    year: values.year,
    month: values.month,
    name: values.name.trim(),
    categoryId: values.categoryId,
    payer: values.payer,
    amount: parseAmount(values.amount) ?? 0,
  };
}
