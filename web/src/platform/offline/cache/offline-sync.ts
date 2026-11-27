import { onlineManager } from '@tanstack/react-query';

/** How often the open app loads the offline data again. */
export const SYNC_INTERVAL_MS = 5 * 60_000;

const visible = () =>
  typeof document === 'undefined' || document.visibilityState === 'visible';

/**
 * Runs `sync` now and whenever the kept data may be old (web ADR 0022): when the network comes back, when the app is
 * shown again, and every {@link SYNC_INTERVAL_MS} while it is open. Nothing runs offline or in a hidden tab. Returns
 * the stop function.
 */
export function startOfflineSync(
  sync: () => void,
  interval = SYNC_INTERVAL_MS,
): () => void {
  const run = () => {
    if (onlineManager.isOnline() && visible()) sync();
  };
  run();
  const stopOnline = onlineManager.subscribe((online) => {
    if (online) run();
  });
  document.addEventListener('visibilitychange', run);
  const timer = setInterval(run, interval);
  return () => {
    stopOnline();
    document.removeEventListener('visibilitychange', run);
    clearInterval(timer);
  };
}
