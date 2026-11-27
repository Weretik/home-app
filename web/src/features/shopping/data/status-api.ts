import { useQueryClient } from '@tanstack/react-query';
import { useQueuedWrite } from '../../../platform/offline/use-queued-write';
import { withStatus, type StatusChange } from './item-status';
import {
  listKey,
  shoppingKey,
  statusMutationKey,
  statusWriteKey,
  type ShoppingList,
} from './shopping-api';

/** The request of a tick: plain data, so it waits offline and is sent after a restart too (web ADR 0020). */
export const statusRequest = ({ id, status, by }: StatusChange) => ({
  method: 'put' as const,
  path: `/api/shopping/items/${encodeURIComponent(id)}/status`,
  body: { status, by },
  module: 'shopping' as const,
  label: status === 'Bought' ? 'позначку «куплено»' : 'повернення до списку',
});

/**
 * Ticks or unticks an item at once (web ADR 0012): the cached list changes before the request, goes back when the
 * request fails, and reloads either way. The API makes a repeat harmless, so the checkbox is never locked. Offline the
 * tick waits in the queue and stays on the screen (web ADR 0020).
 */
export function useSetItemStatus() {
  const queryClient = useQueryClient();
  return useQueuedWrite(statusRequest, {
    key: statusWriteKey,
    onMutate: async (change: StatusChange) => {
      await queryClient.cancelQueries({ queryKey: listKey });
      const before = queryClient.getQueryData<ShoppingList>(listKey);
      if (before) queryClient.setQueryData(listKey, withStatus(before, change));
      return { before };
    },
    onError: (_error, _change, context) => {
      if (context?.before) queryClient.setQueryData(listKey, context.before);
    },
    onSettled: async () => {
      // The last of several quick ticks reloads; earlier ones would bring back a list without the later ticks.
      if (queryClient.isMutating({ mutationKey: statusMutationKey }) <= 1)
        await queryClient.invalidateQueries({ queryKey: shoppingKey });
    },
  });
}
