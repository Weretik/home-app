import { useQuery } from '@tanstack/react-query';
import { apiCall } from '../../../api-client';
import { api } from '../../../platform/api';
import { moduleKeys } from '../../../platform/query-keys';

// TEMP shim (notifications 001): these types and the untyped client below stand in for the generated contract until
// `npm run contracts:generate` runs on the API that has the Notifications module; then they become `Schema<...>`.
export type Audience = 'Vitaliy' | 'Maria' | 'Family';

export interface NotificationChatItem {
  audience: Audience;
  linked: boolean;
  title?: string | null;
  linkedAtUtc?: string | null;
}

export interface NotificationScheduleItem {
  kind: string;
  title: string;
  when: string;
}

export interface NotificationDeliveryItem {
  kind: string;
  audience: Audience;
  key: string;
  createdAtUtc: string;
  sentAtUtc?: string | null;
}

export interface NotificationsStatus {
  configured: boolean;
  botUsername?: string | null;
  botLink?: string | null;
  chats: NotificationChatItem[];
  schedule: NotificationScheduleItem[];
  deliveries: NotificationDeliveryItem[];
}

interface UntypedResult {
  data?: unknown;
  error?: unknown;
  response: Response;
}
/** TEMP shim: the typed client without path checks, for the operations the generated client does not know yet. */
export const untypedApi = api as unknown as {
  GET: (path: string, init?: object) => Promise<UntypedResult>;
  POST: (path: string, init?: object) => Promise<UntypedResult>;
  DELETE: (path: string, init?: object) => Promise<UntypedResult>;
};

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
      apiCall(
        untypedApi.GET('/api/notifications/status', { signal }),
      ) as Promise<NotificationsStatus>,
  });
}
