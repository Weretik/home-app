import type { PaymentMethod } from '../data/expenses-api';
import {
  commentError,
  commentValue,
  dateError,
  todayIso,
  type FieldErrors,
} from '../entry-editing/entry-fields';
import type { RecordPlannedExpenseRequest } from '../planned-data/planned-writes-api';

/** «Записати як витрату»: what the plan does not know yet. Payer, category and amount come from the plan. */
export interface RecordFormValues {
  date: string;
  paymentMethod: PaymentMethod;
  comment: string;
}

/** Paid today, by card, without a comment. */
export const recordFormValues = (
  today: Date = new Date(),
): RecordFormValues => ({
  date: todayIso(today),
  paymentMethod: 'Card',
  comment: '',
});

export function validateRecord(values: RecordFormValues): FieldErrors {
  return {
    date: dateError(values.date),
    comment: commentError(values.comment),
  };
}

export function toRecordRequest(
  values: RecordFormValues,
): RecordPlannedExpenseRequest {
  return {
    date: values.date,
    paymentMethod: values.paymentMethod,
    comment: commentValue(values.comment),
  };
}
