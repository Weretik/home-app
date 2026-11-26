import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/api';
import { ifMatch, useRemove, useSave, type Writer } from '../data/writes-api';
import { budgetKey } from './budget-api';

export type BudgetSettingsRequest = Schema<'BudgetSettingsRequest'>;
export type SpendingLimitRequest = Schema<'SpendingLimitRequest'>;

/**
 * The monthly limit and the savings goal are one record without an id: only the edit is used, with the budget's
 * `version` in If-Match (the id is ignored).
 */
const settingsWriter: Writer<BudgetSettingsRequest> = {
  key: budgetKey,
  create: () => Promise.reject(new Error('Budget settings are only changed.')),
  update: (entry, body) =>
    apiCall(
      api.PUT('/api/expenses/budget/settings', {
        params: { header: { 'If-Match': String(entry.version) } },
        body,
      }),
    ),
  remove: () => Promise.reject(new Error('Budget settings are only changed.')),
};

const limitWriter: Writer<SpendingLimitRequest> = {
  key: budgetKey,
  create: (body, key) =>
    apiCall(
      api.POST('/api/expenses/budget/limits', {
        params: { header: { 'Idempotency-Key': key } },
        body,
      }),
    ),
  update: (entry, body) =>
    apiCall(
      api.PUT('/api/expenses/budget/limits/{id}', {
        params: ifMatch(entry),
        body,
      }),
    ),
  remove: (id) =>
    apiCall(
      api.DELETE('/api/expenses/budget/limits/{id}', {
        params: { path: { id } },
      }),
    ),
};

/** Changes the monthly limit and the savings goal; refreshes the budget. */
export const useSaveBudgetSettings = () => useSave(settingsWriter);
/** Adds or changes a spending limit; refreshes the budget. */
export const useSaveSpendingLimit = () => useSave(limitWriter);
export const useDeleteSpendingLimit = () =>
  useRemove(limitWriter as Writer<unknown>);
