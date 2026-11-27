import { useQueryClient } from '@tanstack/react-query';
import { useEffect, type ReactNode } from 'react';
import { session } from '../../../platform/session/session';
import { SESSION_EXPIRED_TEXT } from '../data/sign-in-errors';
import { useSession } from '../data/use-session';
import { SignInPage } from './SignInPage';

/**
 * Shows the app only with a session token, otherwise the sign-in screen at the same URL, so the page opened is the
 * page shown after sign-in (FR-001, web ADR 0015). Every change of the session (sign-in, sign-out, expiry, another
 * tab) clears the whole query cache, so no data of one session shows in the next.
 */
export function SessionGate({ children }: { children: ReactNode }) {
  const { token, ended } = useSession();
  const queryClient = useQueryClient();

  useEffect(() => session.subscribe(() => queryClient.clear()), [queryClient]);

  if (token) return children;
  return (
    <SignInPage
      clientId={import.meta.env.VITE_GOOGLE_CLIENT_ID || undefined}
      notice={ended === 'expired' ? SESSION_EXPIRED_TEXT : undefined}
      autoSelect={ended !== 'signed-out'}
    />
  );
}
