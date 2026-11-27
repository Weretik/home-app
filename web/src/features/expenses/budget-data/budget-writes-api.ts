import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/client/api';
import {
  ifMatch,
  useChange,
  useRemove,
  useSave,
  type Writer,
} from '../write-data/write-hooks-api';

export type BudgetSettingsRequest = Schema<'BudgetSettingsRequest'>;
export type SpendingLimitRequest = Schema<'SpendingLimitRequest'>;

const limitWriter: Writer<SpendingLimitRequest> = {
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

/**
 * Changes the monthly limit and the savings goal: one record without an id, only ever changed, sent with the budget's
 * `version` in If-Match. Refreshes the Expenses module's queries (the settings, the summary's limits and goal).
 */
export const useSaveBudgetSettings = (version: number) =>
  useChange((body: BudgetSettingsRequest) =>
    apiCall(
      api.PUT('/api/expenses/budget/settings', {
        params: { header: { 'If-Match': String(version) } },
        body,
      }),
    ),
  );
/** Adds or changes a spending limit; refreshes the Expenses module's queries. */
export const useSaveSpendingLimit = () => useSave(limitWriter);
export const useDeleteSpendingLimit = () => useRemove(limitWriter.remove);
