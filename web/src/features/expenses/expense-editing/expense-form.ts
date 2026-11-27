import type { ExpenseItem, Payer, PaymentMethod } from '../data/expenses-api';
import type { ExpenseRequest } from '../data/writes-api';
import {
  amountError,
  amountText,
  commentError,
  commentValue,
  dateError,
  parseAmount,
  type FieldErrors,
} from '../entry-editing/entry-fields';

/** The expense form as typed: text fields stay text until the save. */
export interface ExpenseFormValues {
  date: string;
  categoryId: string;
  payer: Payer;
  amount: string;
  paymentMethod: PaymentMethod;
  comment: string;
  /** «Не в ліміти»: counts in the month's totals, not in any limit or carry-over. */
  outsideLimits: boolean;
}

/** The form of an existing expense, or a new one: shared, by card, within the limits, on `newDate` (the page's default date). */
export function expenseFormValues(
  item: ExpenseItem | undefined,
  newDate: string,
): ExpenseFormValues {
  return item
    ? {
        date: item.date,
        categoryId: item.categoryId,
        payer: item.payer,
        amount: amountText(item.amount),
        paymentMethod: item.paymentMethod,
        comment: item.comment ?? '',
        outsideLimits: item.outsideLimits,
      }
    : {
        date: newDate,
        categoryId: '',
        payer: 'Shared',
        amount: '',
        paymentMethod: 'Card',
        comment: '',
        outsideLimits: false,
      };
}

export function validateExpense(values: ExpenseFormValues): FieldErrors {
  return {
    date: dateError(values.date),
    categoryId: values.categoryId ? undefined : 'Виберіть категорію',
    amount: amountError(values.amount),
    comment: commentError(values.comment),
  };
}

export function toExpenseRequest(values: ExpenseFormValues): ExpenseRequest {
  return {
    date: values.date,
    payer: values.payer,
    categoryId: values.categoryId,
    amount: parseAmount(values.amount) ?? 0,
    paymentMethod: values.paymentMethod,
    comment: commentValue(values.comment),
    outsideLimits: values.outsideLimits,
  };
}
