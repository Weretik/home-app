import { useMutationState } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/client/api';
import {
  queuedWriteKey,
  type QueuedRequest,
  type QueuedVariables,
} from '../../../platform/offline/queue/queued-request';
import { useQueuedWrite } from '../../../platform/offline/queue/use-queued-write';
import {
  ifMatch,
  useRemove,
  useSave,
  type SaveInput,
  type Saver,
  type Writer,
} from '../write-data/write-hooks-api';
import { formatAmount } from './formatting';

export type ExpenseRequest = Schema<'ExpenseRequest'>;
export type IncomeRequest = Schema<'IncomeRequest'>;
export type SavingsMovementRequest = Schema<'SavingsMovementRequest'>;

const expenseWriter: Writer<ExpenseRequest> = {
  create: (body, key) =>
    apiCall(
      api.POST('/api/expenses', {
        params: { header: { 'Idempotency-Key': key } },
        body,
      }),
    ),
  update: (entry, body) =>
    apiCall(api.PUT('/api/expenses/{id}', { params: ifMatch(entry), body })),
  remove: (id) =>
    apiCall(api.DELETE('/api/expenses/{id}', { params: { path: { id } } })),
};

const incomeWriter: Writer<IncomeRequest> = {
  create: (body, key) =>
    apiCall(
      api.POST('/api/expenses/incomes', {
        params: { header: { 'Idempotency-Key': key } },
        body,
      }),
    ),
  update: (entry, body) =>
    apiCall(
      api.PUT('/api/expenses/incomes/{id}', { params: ifMatch(entry), body }),
    ),
  remove: (id) =>
    apiCall(
      api.DELETE('/api/expenses/incomes/{id}', { params: { path: { id } } }),
    ),
};

const savingsWriter: Writer<SavingsMovementRequest> = {
  create: (body, key) =>
    apiCall(
      api.POST('/api/expenses/savings', {
        params: { header: { 'Idempotency-Key': key } },
        body,
      }),
    ),
  update: (entry, body) =>
    apiCall(
      api.PUT('/api/expenses/savings/{id}', { params: ifMatch(entry), body }),
    ),
  remove: (id) =>
    apiCall(
      api.DELETE('/api/expenses/savings/{id}', { params: { path: { id } } }),
    ),
};

/**
 * A new expense as a queued write (web ADR 0022, spec expenses/007): offline it waits and is sent when the network is
 * back, with the form's Idempotency-Key, so a repeat after a lost answer adds one expense only.
 */
export const addExpenseRequest = ({
  body,
  idempotencyKey,
}: SaveInput<ExpenseRequest>): QueuedRequest => ({
  method: 'post',
  path: '/api/expenses',
  body,
  headers: { 'Idempotency-Key': idempotencyKey },
  module: 'expenses',
  label: `витрату ${formatAmount(body.amount)}`,
});

/**
 * Saves an expense: an add is a queued write that also works offline; an edit stays a plain write that needs the
 * network (its version may be stale by the time it is sent). Either refreshes the Expenses module's queries.
 */
export function useSaveExpense(isEdit: boolean): Saver<ExpenseRequest> {
  const change = useSave(expenseWriter);
  const add = useQueuedWrite(addExpenseRequest);
  return isEdit
    ? change
    : { mutate: add.mutate, isPending: add.isSaving, error: add.error };
}
export const useDeleteExpense = () => useRemove(expenseWriter.remove);
/** Adds or changes an income; refreshes the Expenses module's queries. */
export const useSaveIncome = () => useSave(incomeWriter);
export const useDeleteIncome = () => useRemove(incomeWriter.remove);
/** Adds or changes a savings movement; refreshes the Expenses module's queries. */
export const useSaveSavingsMovement = () => useSave(savingsWriter);
export const useDeleteSavingsMovement = () => useRemove(savingsWriter.remove);

/** A new expense that waits for the network; `key` is its Idempotency-Key. */
export interface PendingExpense {
  key: string;
  body: ExpenseRequest;
}

/**
 * The new expenses made offline that wait for the network, oldest first; they are kept with the cache, so they are
 * here also after a restart (spec expenses/007). A write that is being sent online is not listed.
 */
export function usePendingExpenses(): PendingExpense[] {
  const requests = useMutationState({
    filters: { mutationKey: queuedWriteKey, status: 'pending' },
    select: (mutation) =>
      mutation.state.isPaused
        ? (mutation.state.variables as QueuedVariables | undefined)?.request
        : undefined,
  });
  return requests
    .filter(
      (request): request is QueuedRequest =>
        request?.method === 'post' && request.path === '/api/expenses',
    )
    .map((request) => ({
      key: request.headers?.['Idempotency-Key'] ?? '',
      body: request.body as ExpenseRequest,
    }));
}
