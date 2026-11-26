import type { Person } from '../data/expenses-api';
import type { Month } from '../data/month';
import type {
  SavingsDirection,
  SavingsMovementItem,
} from '../data/savings-api';
import type { SavingsMovementRequest } from '../data/writes-api';
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

export const directionLabels: Record<SavingsDirection, string> = {
  Deposit: 'Положили в копилку',
  Withdrawal: 'Взяли из копилки',
};

/** The savings form as typed: the amount is positive, the direction gives its sign. */
export interface SavingsFormValues {
  direction: SavingsDirection;
  date: string;
  person: Person;
  amount: string;
  comment: string;
}

/** The form of an existing movement, or a new deposit by Виталий on the default date. */
export function savingsFormValues(
  item: SavingsMovementItem | undefined,
  month: Month,
): SavingsFormValues {
  return item
    ? {
        direction: item.direction,
        date: item.date,
        person: item.person,
        amount: amountText(item.amount),
        comment: item.comment ?? '',
      }
    : {
        direction: 'Deposit',
        date: defaultEntryDate(month),
        person: 'Vitaliy',
        amount: '',
        comment: '',
      };
}

export function validateSavings(values: SavingsFormValues): FieldErrors {
  return {
    date: dateError(values.date),
    amount: amountError(values.amount),
    comment: commentError(values.comment),
  };
}

/** The request: plus for money put in, minus for money taken out. */
export function toSavingsRequest(
  values: SavingsFormValues,
): SavingsMovementRequest {
  const amount = parseAmount(values.amount) ?? 0;
  return {
    date: values.date,
    person: values.person,
    amount: values.direction === 'Withdrawal' ? -amount : amount,
    comment: commentValue(values.comment),
  };
}
