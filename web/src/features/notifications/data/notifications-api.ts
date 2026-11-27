import { useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/api';
import { moduleKeys } from '../../../platform/query-client';

export type Audience = Schema<'Audience'>;
export type NotificationChatItem = Schema<'NotificationChatItem'>;
export type NotificationScheduleItem = Schema<'NotificationScheduleItem'>;
export type NotificationDeliveryItem = Schema<'NotificationDeliveryItem'>;
export type NotificationsStatus = Schema<'NotificationsStatus'>;

/** Everything the page reads sits under the module prefix, so each write refreshes it (web ADR 0009). */
export const notificationsKeys = {
  all: moduleKeys.notifications,
  status: [...moduleKeys.notifications, 'status'] as const,
};

/** The bot, the linked chats, the schedule and the last deliveries (`getNotificationsStatus`). */
export function useNotificationsStatus() {
  return useQuery({
    queryKey: notificationsKeys.status,
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/notifications/status', { signal })),
  });
}
