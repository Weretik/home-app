import { useQueryClient } from '@tanstack/react-query';
import { useEffect } from 'react';
import { useCurrentMember } from '../../platform/client/member-api';
import { startOfflineSync } from '../../platform/offline/cache/offline-sync';
import { syncOfflineData } from './offline-data';

/**
 * Keeps the main pages' data in the offline cache (web ADR 0022): loads it once the signed-in member is known (the
 * default lists depend on the person), then again when the network is back, when the app is shown and every 5 min.
 */
export function OfflineSync() {
  const queryClient = useQueryClient();
  const member = useCurrentMember();
  const ready = member.isSuccess;
  const person = member.data?.person ?? undefined;

  useEffect(() => {
    if (!ready) return;
    return startOfflineSync(() => syncOfflineData(queryClient, person));
  }, [queryClient, ready, person]);

  return null;
}
