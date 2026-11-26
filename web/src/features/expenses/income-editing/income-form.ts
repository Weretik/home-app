import type { IncomeItem, Person } from '../data/expenses-api';
import type { Month } from '../data/month';
import type { IncomeRequest } from '../data/writes-api';
import {
  amountError,
  amountText,
  commentError,
  commentValue,
  dateError,
  defaultEntryDate,
  parseAmount,
  type FieldErrors,
} from '../entry-editing/entry-fields';

/** The income form as typed. */
export interface IncomeFormValues {
  date: string;
  person: Person;
  amount: string;
  comment: string;
}

/** The form of an existing income, or a new one for Виталий on the default date. */
export function incomeFormValues(
  item: IncomeItem | undefined,
  month: Month,
): IncomeFormValues {
  return item
    ? {
        date: item.date,
        person: item.person,
        amount: amountText(item.amount),
        comment: item.comment ?? '',
      }
    : {
        date: defaultEntryDate(month),
        person: 'Vitaliy',
        amount: '',
        comment: '',
      };
}

export function validateIncome(values: IncomeFormValues): FieldErrors {
  return {
    date: dateError(values.date),
    amount: amountError(values.amount),
    comment: commentError(values.comment),
  };
}

export function toIncomeRequest(values: IncomeFormValues): IncomeRequest {
  return {
    date: values.date,
    person: values.person,
    amount: parseAmount(values.amount) ?? 0,
    comment: commentValue(values.comment),
  };
}
