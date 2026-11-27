import { useMutation, useQueryClient } from '@tanstack/react-query';
import { apiCall } from '../../../api-client';
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
