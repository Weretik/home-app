import { useEffect } from 'react';
import {
  browserPushSupport,
  currentSubscription,
  saveSubscription,
} from './use-push-device';

/**
 * Saves this browser's push subscription again once per start of the signed-in app (notifications 002, FR-107): the
 * API's `last seen` moves, and a device signed in by the other person moves to them. Silent: nothing shows, a failure
 * is retried at the next start. Rendered by the app layout.
 */
export function PushSync() {
  useEffect(() => {
    if (browserPushSupport() !== 'supported') return;
    if (Notification.permission !== 'granted') return;
    currentSubscription()
      .then((subscription) => subscription && saveSubscription(subscription))
      .catch(() => undefined);
  }, []);
  return null;
}
