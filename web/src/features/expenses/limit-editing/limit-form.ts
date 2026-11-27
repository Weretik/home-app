import type { LimitPeriod, SpendingLimitItem } from '../budget-data/budget-api';
import type { SpendingLimitRequest } from '../budget-data/budget-writes-api';
import {
  amountError,
  amountText,
  parseAmount,
  type FieldErrors,
} from '../entry-editing/entry-fields';

/** The API's limit on a name (SpendingLimit.MaxNameLength). */
export const MAX_LIMIT_NAME = 100;

/** The spending limit form as typed. */
export interface LimitFormValues {
  name: string;
  period: LimitPeriod;
  amount: string;
  categoryIds: string[];
}

/** The form of an existing limit, or a new monthly one without categories. */
export function limitFormValues(item?: SpendingLimitItem): LimitFormValues {
  return item
    ? {
        name: item.name,
        period: item.period,
        amount: amountText(item.amount),
        categoryIds: [...item.categoryIds],
      }
    : { name: '', period: 'Month', amount: '', categoryIds: [] };
}

export function validateLimit(values: LimitFormValues): FieldErrors {
  const name = values.name.trim();
  return {
    name: !name
      ? 'Введіть назву'
      : name.length > MAX_LIMIT_NAME
        ? `Не довше ${MAX_LIMIT_NAME} символів`
        : undefined,
    amount: amountError(values.amount),
    categoryIds:
      values.categoryIds.length === 0
        ? 'Виберіть хоча б одну категорію'
        : undefined,
  };
}

export function toLimitRequest(values: LimitFormValues): SpendingLimitRequest {
  return {
    name: values.name.trim(),
    period: values.period,
    amount: parseAmount(values.amount) ?? 0,
    categoryIds: [...new Set(values.categoryIds)],
  };
}
