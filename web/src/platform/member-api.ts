import { useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../api-client';
import { api } from './api';
import { moduleKeys } from './query-client';

export type FamilyMember = Schema<'FamilyMember'>;
/** Who the signed-in member is in the home; the same values as the people of the other modules. */
export type FamilyPerson = NonNullable<FamilyMember['person']>;

export const memberKeys = {
  me: [...moduleKeys.identity, 'me'] as const,
};

/**
 * The signed-in member (`getCurrentMember`); it does not change during a session. In platform/ because every section
 * starts on the signed-in person (web ADR 0016) and features never import each other.
 */
export function useCurrentMember() {
  return useQuery({
    queryKey: memberKeys.me,
    queryFn: ({ signal }) => apiCall(api.GET('/api/identity/me', { signal })),
    staleTime: Infinity,
  });
}

/** «Віталій» or «Маша» as signed in, or undefined while loading or for a member who is neither. */
export function useSignedInPerson(): FamilyPerson | undefined {
  return useCurrentMember().data?.person ?? undefined;
}
