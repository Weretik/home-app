import { useSyncExternalStore } from 'react';
import { session, type SessionState } from '../../../platform/session';

/** The session token store as React state: re-renders on sign-in, sign-out, expiry and changes in other tabs. */
export function useSession(): SessionState {
  return useSyncExternalStore(session.subscribe, session.current);
}
