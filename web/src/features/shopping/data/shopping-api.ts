import { useQuery, useQueryClient } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/api';
import { moduleKeys } from '../../../platform/query-keys';

export type ShoppingItem = Schema<'ShoppingItemView'>;
export type ShoppingList = Schema<'ShoppingList'>;
export type FrequentItem = Schema<'FrequentItem'>;
export type DepartmentMatch = Schema<'DepartmentMatch'>;
export type ShoppingPerson = ShoppingItem['addedBy'];

/** Every shopping query starts with this key, so an item write refreshes the list, history, frequent and counts. */
export const shoppingKey = moduleKeys.shopping;
export const listKey = [...shoppingKey, 'list'] as const;
/** The key of the checkbox mutation; polling pauses while one is pending so it cannot undo the optimistic tick. */
export const statusMutationKey = [...shoppingKey, 'status'] as const;

/** How often the open list asks for the other person's changes. */
export const POLL_MS = 20_000;

/**
 * The list (needed items and the cart). It reloads every 20 s while the page is visible (not in a background tab), on
 * window focus and after every write; a pending checkbox pauses the reload (web ADR 0012).
 */
export function useShoppingList() {
  const queryClient = useQueryClient();
  return useQuery({
    queryKey: listKey,
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/shopping/items', { signal })),
    // Always stale, so a return to the tab reloads at once (the app's default keeps data fresh for 30 s).
    staleTime: 0,
    refetchInterval: () =>
      queryClient.isMutating({ mutationKey: statusMutationKey }) > 0
        ? false
        : POLL_MS,
    refetchIntervalInBackground: false,
    refetchOnWindowFocus: true,
  });
}

/** ⭐ Частые: names often bought lately that are not in the list now. */
export function useFrequentItems() {
  return useQuery({
    queryKey: [...shoppingKey, 'frequent'],
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/shopping/frequent', { signal })),
  });
}

/** The department a single item would go to; nothing is asked for an empty name. */
export function useDepartmentMatch(name: string) {
  return useQuery({
    queryKey: [...shoppingKey, 'match', name],
    enabled: name.length > 0,
    staleTime: 5 * 60_000,
    queryFn: ({ signal }) =>
      apiCall(
        api.GET('/api/shopping/departments/match', {
          params: { query: { name } },
          signal,
        }),
      ),
  });
}
