import { useRegisterSW } from 'virtual:pwa-register/react';

/** How often an open app asks whether a new version was deployed. */
export const UPDATE_CHECK_MS = 60 * 60 * 1000;

/**
 * The service worker's registration (web ADR 0019): it is registered once, checks for a new deploy every hour, and a
 * new version waits until the person taps «Оновити» (nothing reloads on its own, so a half-filled form is kept).
 */
export function useAppUpdate() {
  const {
    needRefresh: [needRefresh, setNeedRefresh],
    updateServiceWorker,
  } = useRegisterSW({
    onRegisteredSW(_url, registration) {
      if (!registration) return;
      setInterval(() => void registration.update(), UPDATE_CHECK_MS);
    },
  });

  return {
    needRefresh,
    update: () => void updateServiceWorker(true),
    dismiss: () => setNeedRefresh(false),
  };
}
