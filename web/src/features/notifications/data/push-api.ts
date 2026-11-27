import {
  useMutation,
  useQuery,
  useQueryClient,
  queryOptions,
} from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/client/api';
import { notificationsKeys } from './notifications-api';

export type PushStatus = Schema<'PushStatus'>;
export type PushDevice = Schema<'PushDevice'>;
export type SavePushSubscriptionRequest = Schema<'SavePushSubscriptionRequest'>;

export const pushKey = [...notificationsKeys.all, 'push'] as const;

/** The query of `usePushStatus`, also used to load it ahead for offline use (web ADR 0022). */
export const pushStatusQuery = () =>
  queryOptions({
    queryKey: pushKey,
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/notifications/push', { signal })),
  });

/** Whether push is set up, the key to subscribe with and the signed-in person's devices (`getPushStatus`). */
export function usePushStatus() {
  return useQuery({ ...pushStatusQuery() });
}

/** Saves this browser's subscription; the API makes it an upsert by endpoint, so a repeat is harmless (R-102). */
export const savePushSubscription = (body: SavePushSubscriptionRequest) =>
  apiCall(api.PUT('/api/notifications/push/subscriptions', { body }));

/** Removes one of the person's devices (`deletePushSubscription`). */
export const deletePushDevice = (id: string) =>
  apiCall(
    api.DELETE('/api/notifications/push/subscriptions/{id}', {
      params: { path: { id } },
    }),
  );

function useRefresh() {
  const queryClient = useQueryClient();
  return () =>
    void queryClient.invalidateQueries({ queryKey: notificationsKeys.all });
}

/** «Видалити» of another device. */
export function useDeletePushDevice() {
  const refresh = useRefresh();
  return useMutation({ mutationFn: deletePushDevice, onSuccess: refresh });
}

/** «Надіслати тест»: a push to every device of the person at once; nothing stored, so no Idempotency-Key. */
export function useSendPushTest() {
  return useMutation({
    mutationFn: () => apiCall(api.POST('/api/notifications/push/test', {})),
  });
}

/** Wraps a browser-side change of this device (switch on or off), refreshing the device list after it. */
export function usePushChange<Input>(fn: (input: Input) => Promise<unknown>) {
  const refresh = useRefresh();
  return useMutation({ mutationFn: fn, onSettled: refresh });
}
