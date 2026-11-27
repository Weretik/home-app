import { onlineManager } from '@tanstack/react-query';

/** How long the app counts itself offline after a request got no answer, before it tries again. */
export const OFFLINE_PROBE_MS = 15_000;

let probe: ReturnType<typeof setTimeout> | undefined;

/**
 * A request got no answer, although the browser may still say it is online (a network without internet, a weak
 * signal): the app counts itself offline, so reads keep the data they have and pause instead of failing, queued
 * writes wait, and the top bar says «Офлайн». After {@link OFFLINE_PROBE_MS} it counts itself online again and the
 * paused requests retry; another failure starts the wait again. The browser's own `online` event ends it at once.
 */
export function markOffline() {
  onlineManager.setOnline(false);
  clearTimeout(probe);
  probe = setTimeout(() => {
    probe = undefined;
    onlineManager.setOnline(true);
  }, OFFLINE_PROBE_MS);
}

/** Forgets a pending probe and counts the app online (tests, sign-out). */
export function resetConnectivity() {
  clearTimeout(probe);
  probe = undefined;
  onlineManager.setOnline(true);
}
