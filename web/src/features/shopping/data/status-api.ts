import { useMutation, useQueryClient } from '@tanstack/react-query';
import { apiCall } from '../../../api-client';
import { api } from '../../../platform/api';
import { withStatus, type StatusChange } from './item-status';
import {
  listKey,
  shoppingKey,
  statusMutationKey,
  type ShoppingList,
} from './shopping-api';

/**
 * Ticks or unticks an item at once (web ADR 0012): the cached list changes before the request, goes back when the
 * request fails, and reloads either way. The API makes a repeat harmless, so the checkbox is never locked.
 */
export function useSetItemStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationKey: statusMutationKey,
    mutationFn: ({ id, status, by }: StatusChange) =>
      apiCall(
        api.PUT('/api/shopping/items/{id}/status', {
          params: { path: { id } },
          body: { status, by },
        }),
      ),
    onMutate: async (change) => {
      await queryClient.cancelQueries({ queryKey: listKey });
      const before = queryClient.getQueryData<ShoppingList>(listKey);
      if (before) queryClient.setQueryData(listKey, withStatus(before, change));
      return { before };
    },
    onError: (_error, _change, context) => {
      if (context?.before) queryClient.setQueryData(listKey, context.before);
    },
    onSettled: () => {
      // The last of several quick ticks reloads; earlier ones would bring back a list without the later ticks.
      if (queryClient.isMutating({ mutationKey: statusMutationKey }) <= 1)
        void queryClient.invalidateQueries({ queryKey: shoppingKey });
    },
  });
}
