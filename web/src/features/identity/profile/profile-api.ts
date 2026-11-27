import { useQuery } from '@tanstack/react-query';
import { apiCall } from '../../../api-client';
import { api } from '../../../platform/client/api';
import type { FamilyPerson } from '../../../platform/client/member-api';
import { moduleKeys } from '../../../platform/client/query-client';

/**
 * The signed-in person's private Telegram chat for the profile (identity 002 FR-005), read from the notifications
 * status (`getNotificationsStatus`) under the same key as the «Телеграм» page, so a link or unlink there refreshes it.
 * Undefined data while there is no person to look up.
 */
export function useTelegramChat(person: FamilyPerson | undefined) {
  return useQuery({
    queryKey: [...moduleKeys.notifications, 'status'],
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/notifications/status', { signal })),
    enabled: person !== undefined,
    select: (status) => status.chats.find((chat) => chat.audience === person),
  });
}
