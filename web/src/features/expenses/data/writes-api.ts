import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/api';
import {
  ifMatch,
  useRemove,
  useSave,
  type Writer,
} from '../write-data/write-hooks-api';

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

/** Adds or changes an expense; refreshes the Expenses module's queries. */
export const useSaveExpense = () => useSave(expenseWriter);
export const useDeleteExpense = () => useRemove(expenseWriter.remove);
/** Adds or changes an income; refreshes the Expenses module's queries. */
export const useSaveIncome = () => useSave(incomeWriter);
export const useDeleteIncome = () => useRemove(incomeWriter.remove);
/** Adds or changes a savings movement; refreshes the Expenses module's queries. */
export const useSaveSavingsMovement = () => useSave(savingsWriter);
export const useDeleteSavingsMovement = () => useRemove(savingsWriter.remove);
