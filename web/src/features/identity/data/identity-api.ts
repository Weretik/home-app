import { useMutation, useQueryClient } from '@tanstack/react-query';
import { ApiError, apiCall } from '../../../api-client';
import { api } from '../../../platform/client/api';
import { memberKeys } from '../../../platform/client/member-api';
import { session } from '../../../platform/session/session';

export {
  useCurrentMember,
  type FamilyMember,
} from '../../../platform/client/member-api';

/**
 * Exchanges Google's ID token for a session (`signIn`) and keeps its token. No Idempotency-Key: a repeat only makes
 * one more session (backend SDD identity 001, "Contract"). The whole cache is cleared, so nothing cached before
 * the sign-in shows up in the new session.
 */
export function useSignIn() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (credential: string) =>
      apiCall(api.POST('/api/identity/sign-in', { body: { credential } })),
    onSuccess: (signedIn) => {
      session.save(signedIn.token);
      queryClient.clear();
      queryClient.setQueryData(memberKeys.me, signedIn.member);
    },
  });
}

/**
 * «Вийти»: ends the session on the server (best effort: a failure still signs this device out), forgets the token
 * and clears the cache.
 */
export function useSignOut() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async () => {
      try {
        await apiCall(api.POST('/api/identity/sign-out'));
      } catch {
        // The token is forgotten anyway; the server ends an unused session by itself after 30 days.
      }
    },
    onSettled: () => {
      session.clear('signed-out');
      queryClient.clear();
    },
  });
}

/**
 * «Вийти на всіх пристроях» (ADR 0046): ends every session of the member on the server, then forgets this device's
 * token and clears the cache. Unlike «Вийти» it is not best effort: if the server did not confirm, the other devices
 * may still be signed in, so the error is shown and nothing is forgotten. A `401` means this session had already
 * ended, so this device signs out too.
 */
export function useSignOutEverywhere() {
  const queryClient = useQueryClient();
  const forget = () => {
    session.clear('signed-out');
    queryClient.clear();
  };
  return useMutation({
    mutationFn: () => apiCall(api.POST('/api/identity/sign-out-everywhere')),
    onSuccess: forget,
    onError: (error) => {
      if (error instanceof ApiError && error.status === 401) forget();
    },
  });
}
