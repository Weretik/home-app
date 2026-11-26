import {
  useMutation,
  useQueryClient,
  type QueryKey,
} from '@tanstack/react-query';
import { useRef } from 'react';
import {
  ApiError,
  apiCall,
  newIdempotencyKey,
  type Schema,
} from '../../../api-client';
import { api } from '../../../platform/api';
import { savingsKey } from './savings-api';

export type ExpenseRequest = Schema<'ExpenseRequest'>;
export type IncomeRequest = Schema<'IncomeRequest'>;
export type SavingsMovementRequest = Schema<'SavingsMovementRequest'>;

/** The entry an edit starts from: its id and the version the list showed (sent in If-Match). */
export interface EditedEntry {
  id: string;
  version: number;
}

export interface SaveInput<Body> {
  body: Body;
  /** Set for an edit; a create leaves it out. */
  edit?: EditedEntry;
  /** Sent with a create (see {@link useIdempotencyKey}). */
  idempotencyKey: string;
}

interface Writer<Body> {
  /** The queries the entry shows in; a write refreshes them. */
  key: QueryKey;
  create: (body: Body, idempotencyKey: string) => Promise<unknown>;
  update: (entry: EditedEntry, body: Body) => Promise<unknown>;
  remove: (id: string) => Promise<unknown>;
}

const ifMatch = (entry: EditedEntry) => ({
  path: { id: entry.id },
  header: { 'If-Match': String(entry.version) },
});

const expenseWriter: Writer<ExpenseRequest> = {
  key: ['expenses', 'list'],
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
  key: ['expenses', 'incomes'],
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
  key: savingsKey,
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

/** The entry is no longer what the list shows (changed or deleted elsewhere), so the list must reload. */
const isStale = (error: Error) =>
  error instanceof ApiError &&
  (error.status === 404 || error.code === 'expenses.changed_meanwhile');

function useSave<Body>(writer: Writer<Body>) {
  const queryClient = useQueryClient();
  const refresh = () =>
    void queryClient.invalidateQueries({ queryKey: writer.key });
  return useMutation({
    mutationFn: ({ body, edit, idempotencyKey }: SaveInput<Body>) =>
      edit ? writer.update(edit, body) : writer.create(body, idempotencyKey),
    onSuccess: refresh,
    onError: (error) => {
      if (isStale(error)) refresh();
    },
  });
}

function useRemove(writer: Writer<unknown>) {
  const queryClient = useQueryClient();
  const refresh = () =>
    void queryClient.invalidateQueries({ queryKey: writer.key });
  return useMutation({
    mutationFn: (id: string) => writer.remove(id),
    onSuccess: refresh,
    onError: (error) => {
      if (isStale(error)) refresh();
    },
  });
}

/** Adds or changes an expense; refreshes the expense lists. */
export const useSaveExpense = () => useSave(expenseWriter);
export const useDeleteExpense = () =>
  useRemove(expenseWriter as Writer<unknown>);
/** Adds or changes an income; refreshes the income lists. */
export const useSaveIncome = () => useSave(incomeWriter);
export const useDeleteIncome = () => useRemove(incomeWriter as Writer<unknown>);
/** Adds or changes a savings movement; refreshes the movements and the months. */
export const useSaveSavingsMovement = () => useSave(savingsWriter);
export const useDeleteSavingsMovement = () =>
  useRemove(savingsWriter as Writer<unknown>);

/**
 * The Idempotency-Key of one opened add form (docs/rules.md, "Writes"): the same key while the same data is retried,
 * a new one when the data changes. A form that is opened again mounts again and starts with a new key.
 */
export function useIdempotencyKey() {
  const last = useRef<{ body: string; key: string } | null>(null);
  return {
    keyFor: (body: unknown): string => {
      const text = JSON.stringify(body);
      if (last.current?.body !== text)
        last.current = { body: text, key: newIdempotencyKey() };
      return last.current.key;
    },
    /** Forgets the key, after the API said it was used for other data. */
    renew: () => {
      last.current = null;
    },
  };
}

/** What a form shows after a failed write: one message, and the messages of the fields the API rejected. */
export interface WriteFailure {
  message: string;
  fieldErrors: Record<string, string[]>;
  /** The Idempotency-Key was already used for other data; the next attempt needs a new one. */
  renewKey?: boolean;
}

/** Turns a failed write into Russian text for the form (docs/rules.md, "API"); `action` names the failed step. */
export function describeWriteError(
  error: Error,
  action = 'сохранить',
): WriteFailure {
  if (!(error instanceof ApiError))
    return {
      message: 'Нет связи с сервером. Проверьте интернет и попробуйте снова.',
      fieldErrors: {},
    };
  const fieldErrors = error.fieldErrors;
  if (error.code === 'expenses.changed_meanwhile')
    return {
      message:
        'Запись изменили в другом месте — обновите и попробуйте снова. Список уже обновлён.',
      fieldErrors,
    };
  if (error.status === 404)
    return {
      message: 'Этой записи уже нет: её удалили в другом месте.',
      fieldErrors,
    };
  if (error.status === 409)
    return {
      message:
        'Эту запись уже отправляли с другими данными. Попробуйте ещё раз.',
      fieldErrors,
      renewKey: true,
    };
  if (error.status === 429)
    return {
      message: 'Слишком много изменений подряд. Подождите минуту и повторите.',
      fieldErrors,
    };
  if (error.status === 400)
    return { message: `Проверьте поля: ${error.message}`, fieldErrors };
  return { message: `Не удалось ${action}: ${error.message}`, fieldErrors };
}
