/// <reference lib="webworker" />
import { clientsClaim } from 'workbox-core';
import {
  cleanupOutdatedCaches,
  createHandlerBoundToURL,
  precacheAndRoute,
} from 'workbox-precaching';
import { NavigationRoute, registerRoute } from 'workbox-routing';
import {
  clickTarget,
  notificationOptions,
  parsePushPayload,
  sameOriginClient,
} from './push-notification';

/**
 * The service worker (web ADR 0021). It precaches the built shell, answers every navigation with the cached
 * index.html, and shows the API's push messages. API calls go to another origin and are never cached here; the data
 * offline comes from the React Query cache in IndexedDB (web ADR 0022). A new version waits until the update prompt
 * sends SKIP_WAITING.
 */
declare const self: ServiceWorkerGlobalScope & {
  __WB_MANIFEST: Array<{ url: string; revision: string | null } | string>;
};

precacheAndRoute(self.__WB_MANIFEST);
cleanupOutdatedCaches();
registerRoute(new NavigationRoute(createHandlerBoundToURL('/index.html')));

self.addEventListener('message', (event) => {
  if (event.data?.type === 'SKIP_WAITING') void self.skipWaiting();
});

// Once the new version took over (after «Оновити»), it controls the open pages at once.
clientsClaim();

self.addEventListener('push', (event) => {
  const message = parsePushPayload(event.data?.text());
  event.waitUntil(
    self.registration.showNotification(
      message.title,
      notificationOptions(message),
    ),
  );
});

async function openTarget(url: unknown) {
  const origin = self.location.origin;
  const target = clickTarget(url, origin);
  const windows = await self.clients.matchAll({
    type: 'window',
    includeUncontrolled: true,
  });
  const client = sameOriginClient(windows, origin);
  if (client) {
    const focused = await client.focus();
    const navigated = await focused.navigate(target).catch(() => null);
    if (navigated) return;
  }
  await self.clients.openWindow(target);
}

self.addEventListener('notificationclick', (event) => {
  event.notification.close();
  event.waitUntil(
    openTarget((event.notification.data as { url?: unknown } | null)?.url),
  );
});
