import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/client/api';
import {
  ifMatch,
  useChange,
  useRemove,
  useSave,
  type EditedEntry,
  type Writer,
} from '../write-data/write-hooks-api';

export type PlannedExpenseRequest = Schema<'PlannedExpenseRequest'>;
export type RecordPlannedExpenseRequest = Schema<'RecordPlannedExpenseRequest'>;

const plannedWriter: Writer<PlannedExpenseRequest> = {
  create: (body, key) =>
    apiCall(
      api.POST('/api/expenses/planned', {
        params: { header: { 'Idempotency-Key': key } },
        body,
      }),
    ),
  update: (entry, body) =>
    apiCall(
      api.PUT('/api/expenses/planned/{id}', { params: ifMatch(entry), body }),
    ),
  remove: (id) =>
    apiCall(
      api.DELETE('/api/expenses/planned/{id}', { params: { path: { id } } }),
    ),
};

/** Adds or changes a planned expense; refreshes the Expenses module's queries. */
export const useSavePlannedExpense = () => useSave(plannedWriter);
export const useDeletePlannedExpense = () => useRemove(plannedWriter.remove);

/**
 * «Записати як витрату»: turns `plan` into an expense and deletes the plan in one step, with the plan's version in
 * If-Match. It changes the plans, the expenses and the summary; like every write it refreshes the module's queries.
 */
export const useRecordPlannedExpense = (plan: EditedEntry) =>
  useChange((body: RecordPlannedExpenseRequest) =>
    apiCall(
      api.POST('/api/expenses/planned/{id}/record', {
        params: ifMatch(plan),
        body,
      }),
    ),
  );
