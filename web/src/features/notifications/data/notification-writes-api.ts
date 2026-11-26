import { useMutation, useQueryClient } from '@tanstack/react-query';
import { apiCall } from '../../../api-client';
import {
  notificationsKeys,
  untypedApi,
  type Audience,
} from './notifications-api';

function useRefresh() {
  const queryClient = useQueryClient();
  return () =>
    void queryClient.invalidateQueries({ queryKey: notificationsKeys.all });
}

/** «Отвязать»: forgets the chat of a person or the family group (`unlinkNotificationsChat`). */
export function useUnlinkChat() {
  const refresh = useRefresh();
  return useMutation({
    mutationFn: (audience: Audience) =>
      apiCall(
        untypedApi.DELETE('/api/notifications/chats/{audience}', {
          params: { path: { audience } },
        }),
      ),
    onSuccess: refresh,
  });
}

/**
 * «Тест»: the API sends «✅ Бот на связи» to a linked chat at once (`sendNotificationsTest`). Nothing is stored, so no
 * Idempotency-Key: a repeat only sends one more test message (backend SDD notifications 001, "Contract").
 */
export function useSendTest() {
  return useMutation({
    mutationFn: (audience: Audience) =>
      apiCall(
        untypedApi.POST('/api/notifications/test', { body: { audience } }),
      ),
  });
}
