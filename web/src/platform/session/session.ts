/**
 * The session token store (web ADR 0015): the API's session token in localStorage (`home.session`), read
 * synchronously, so the first render already knows whether to show the app. The API client adds the token to every
 * call; `features/identity` shows the sign-in screen while there is none. Storage access is wrapped: in a private
 * window or with blocked site data the token lives in memory for this tab only.
 */
export type SessionEndReason = 'expired' | 'signed-out';

export interface SessionState {
  /** The bearer token of the API session, or null when signed out. */
  token: string | null;
  /**
   * Why the last session ended; null while signed in or before any session. A token removed by another tab counts
   * as signed out.
   */
  ended: SessionEndReason | null;
}

export const SESSION_STORAGE_KEY = 'home.session';

let state: SessionState = { token: null, ended: null };
let memory: string | null = null;
let storageWorks = true;
const listeners = new Set<() => void>();

function readStored(): string | null {
  if (!storageWorks) return memory;
  try {
    return localStorage.getItem(SESSION_STORAGE_KEY);
  } catch {
    storageWorks = false;
    return memory;
  }
}

function writeStored(token: string | null) {
  memory = token;
  if (!storageWorks) return;
  try {
    if (token === null) localStorage.removeItem(SESSION_STORAGE_KEY);
    else localStorage.setItem(SESSION_STORAGE_KEY, token);
  } catch {
    storageWorks = false;
  }
}

function notify() {
  for (const listener of listeners) listener();
}

/** The current state; the same object until the token changes, as `useSyncExternalStore` needs. */
function current(): SessionState {
  const stored = readStored();
  if (stored !== state.token)
    state = {
      token: stored,
      ended: stored !== null ? null : state.token ? 'signed-out' : state.ended,
    };
  return state;
}

function onStorage(event: StorageEvent) {
  if (event.key !== null && event.key !== SESSION_STORAGE_KEY) return;
  const before = state;
  if (current() !== before) notify();
}

export const session = {
  current,

  /** The stored token, or null. */
  token: (): string | null => current().token,

  /** Keeps the token of a new session. */
  save(token: string) {
    writeStored(token);
    state = { token, ended: null };
    notify();
  },

  /** Forgets the token: the session expired (a 401) or the member signed out. */
  clear(reason: SessionEndReason) {
    writeStored(null);
    state = { token: null, ended: reason };
    notify();
  },

  /** Calls the listener on every change, also a sign-in or sign-out in another tab; returns the unsubscribe. */
  subscribe(listener: () => void): () => void {
    listeners.add(listener);
    if (listeners.size === 1) window.addEventListener('storage', onStorage);
    return () => {
      listeners.delete(listener);
      if (listeners.size === 0)
        window.removeEventListener('storage', onStorage);
    };
  },
};
