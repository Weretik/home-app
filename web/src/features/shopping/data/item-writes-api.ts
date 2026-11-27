import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useRef } from 'react';
import { apiCall, newIdempotencyKey, type Schema } from '../../../api-client';
import { api } from '../../../platform/api';
import { shoppingKey, type ShoppingItem } from './shopping-api';
import { isStale } from './write-failure-api';

export type AddItemsRequest = Schema<'AddShoppingItemsRequest'>;
export type AddItemsResult = Schema<'AddItemsResult'>;
export type UpdateItemRequest = Schema<'UpdateShoppingItemRequest'>;

/** An existing row: its id and the version the page showed (sent in If-Match). */
export interface Versioned {
  id: string;
  version: number;
}

export const ifMatch = (row: Versioned) => ({
  path: { id: row.id },
  header: { 'If-Match': String(row.version) },
});

/**
 * A shopping write that refreshes every shopping query after success, and after a failure that says the page was out
 * of date. Departments and keywords use it too: a renamed department shows in the list.
 */
export function useShoppingWrite<Input, Output>(
  fn: (input: Input) => Promise<Output>,
) {
  const queryClient = useQueryClient();
  const refresh = () =>
    void queryClient.invalidateQueries({ queryKey: shoppingKey });
  return useMutation({
    mutationFn: fn,
    onSuccess: refresh,
    onError: (error) => {
      if (isStale(error)) refresh();
    },
  });
}

export interface AddInput {
  body: AddItemsRequest;
  idempotencyKey: string;
}

/** Adds items from text («молоко, хлеб, яйца 10»); the API splits it, finds departments and merges repeats. */
export const useAddItems = () =>
  useShoppingWrite(({ body, idempotencyKey }: AddInput) =>
    apiCall(
      api.POST('/api/shopping/items', {
        params: { header: { 'Idempotency-Key': idempotencyKey } },
        body,
      }),
    ),
  );

/** Changes an item's name, quantity, note, department or «Терміново». */
export const useUpdateItem = () =>
  useShoppingWrite(
    ({ row, body }: { row: Versioned; body: UpdateItemRequest }) =>
      apiCall(
        api.PUT('/api/shopping/items/{id}', { params: ifMatch(row), body }),
      ),
  );

const deleteItem = (row: Versioned) =>
  apiCall(api.DELETE('/api/shopping/items/{id}', { params: ifMatch(row) }));

/** Deletes an item of the list or a history row. */
export const useDeleteItem = () => useShoppingWrite(deleteItem);

/** «Скасувати» after an add: deletes the rows the add created (restored and updated ones stay). */
export const useUndoAdd = () =>
  useShoppingWrite((added: ShoppingItem[]) =>
    Promise.all(added.map((item) => deleteItem(item))),
  );

/** «Прибрати куплене»: the whole cart goes to the history. */
export const useClearCart = () =>
  useShoppingWrite(() =>
    apiCall(api.POST('/api/shopping/items/clear-cart', {})),
  );

/**
 * The Idempotency-Key of an add (docs/rules.md, "Writes"): the same key while the same data is retried, a new one
 * when the data changes or after a success (`renew`).
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
    renew: () => {
      last.current = null;
    },
  };
}
