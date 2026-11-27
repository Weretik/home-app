import { useEffect, useState } from 'react';
import {
  deletePushDevice,
  savePushSubscription,
  usePushChange,
  type PushDevice,
} from '../data/push-api';
import {
  decodeKey,
  deviceLabel,
  pushSupport,
  subscriptionBody,
  type PushSupport,
} from './push-support';

/** This browser's traits, read once; safe where `navigator` lacks the push APIs (tests, old browsers). */
export function browserPushSupport(): PushSupport {
  return pushSupport({
    userAgent: navigator.userAgent,
    standalone:
      window.matchMedia?.('(display-mode: standalone)').matches === true ||
      (navigator as Navigator & { standalone?: boolean }).standalone === true,
    hasServiceWorker: 'serviceWorker' in navigator,
    hasPushManager: 'PushManager' in window,
    hasNotification: 'Notification' in window,
  });
}

/** The browser's current push subscription, or null (also when the service worker is not ready). */
export async function currentSubscription(): Promise<PushSubscription | null> {
  const registration = await navigator.serviceWorker.ready;
  return registration.pushManager.getSubscription();
}

/** Saves an existing subscription to the API for the person signed in now (FR-107). */
export const saveSubscription = (subscription: PushSubscription) =>
  savePushSubscription(
    subscriptionBody(subscription.toJSON(), deviceLabel(navigator.userAgent)),
  );

/** Thrown when the person refused the notification permission (FR-104). */
export class PermissionDeniedError extends Error {
  constructor() {
    super('Дозвіл на сповіщення не надано');
  }
}

async function enable(publicKey: string) {
  const permission = await Notification.requestPermission();
  if (permission !== 'granted') throw new PermissionDeniedError();
  const registration = await navigator.serviceWorker.ready;
  const subscription =
    (await registration.pushManager.getSubscription()) ??
    (await registration.pushManager.subscribe({
      userVisibleOnly: true,
      applicationServerKey: decodeKey(publicKey),
    }));
  await saveSubscription(subscription);
}

async function disable(devices: readonly PushDevice[]) {
  const subscription = await currentSubscription();
  if (!subscription) return;
  const device = devices.find((d) => d.endpoint === subscription.endpoint);
  await subscription.unsubscribe();
  if (device) await deletePushDevice(device.id);
}

/**
 * This device and push (notifications 002, FR-103, FR-104): what the browser can do, whether it is subscribed (its
 * endpoint), and the switch's on and off.
 */
export function usePushDevice(devices: readonly PushDevice[]) {
  const [support] = useState(browserPushSupport);
  const [endpoint, setEndpoint] = useState<string | null>(null);
  const turnOn = usePushChange(enable);
  const turnOff = usePushChange(() => disable(devices));

  useEffect(() => {
    if (support !== 'supported') return;
    let live = true;
    currentSubscription().then(
      (subscription) => live && setEndpoint(subscription?.endpoint ?? null),
      () => undefined,
    );
    return () => {
      live = false;
    };
  }, [support, turnOn.status, turnOff.status]);

  const thisDevice = devices.find((d) => d.endpoint === endpoint) ?? null;
  return {
    support,
    denied: 'Notification' in window && Notification.permission === 'denied',
    thisDevice,
    on: thisDevice !== null,
    pending: turnOn.isPending || turnOff.isPending,
    error: turnOn.error ?? turnOff.error,
    enable: (publicKey: string) => turnOn.mutate(publicKey),
    disable: () => turnOff.mutate(undefined),
  };
}
