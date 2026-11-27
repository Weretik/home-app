import type { LimitPeriod, SpendingLimitItem } from '../budget-data/budget-api';
import type { SpendingLimitRequest } from '../budget-data/budget-writes-api';
import { kyivMonthText, monthStartDay } from '../data/month';
import {
  amountError,
  amountText,
  monthError,
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
  /** «Діє з»: the month («YYYY-MM») the amount applies from; only when editing, a new limit has none. */
  amountFrom?: string;
}

/**
 * The form of an existing limit (its current amount, applying from the current month in Kyiv), or a new monthly one
 * without categories.
 */
export function limitFormValues(
  item?: SpendingLimitItem,
  now: Date = new Date(),
): LimitFormValues {
  return item
    ? {
        name: item.name,
        period: item.period,
        amount: amountText(item.amount),
        categoryIds: [...item.categoryIds],
        amountFrom: kyivMonthText(now),
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
    amountFrom:
      values.amountFrom === undefined
        ? undefined
        : monthError(values.amountFrom),
  };
}

/**
 * Whether an edit sends «Діє з»: only when the amount differs from the one the form started with (the limit's current
 * amount) or the month was moved off the default. An explicit `amountFrom` replaces every later step on the server
 * (api R-LA-002), so a rename must leave it out (web SDD 006, R-WLA-001).
 */
export function sendsAmountFrom(
  values: LimitFormValues,
  initial?: LimitFormValues,
): boolean {
  if (values.amountFrom === undefined) return false;
  if (initial?.amountFrom === undefined) return true;
  return (
    parseAmount(values.amount) !== parseAmount(initial.amount) ||
    values.amountFrom !== initial.amountFrom
  );
}

/**
 * The request; `amountFrom` (the month's first day, the server moves it to the quarter's) only for an edit whose
 * amount or month changed against `initial`, the form's starting values.
 */
export function toLimitRequest(
  values: LimitFormValues,
  initial?: LimitFormValues,
): SpendingLimitRequest {
  return {
    name: values.name.trim(),
    period: values.period,
    amount: parseAmount(values.amount) ?? 0,
    categoryIds: [...new Set(values.categoryIds)],
    ...(values.amountFrom !== undefined &&
      sendsAmountFrom(values, initial) && {
        amountFrom: monthStartDay(values.amountFrom),
      }),
  };
}
