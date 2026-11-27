import { useCurrentMember } from '../../../platform/member-api';

/**
 * The signed-in person (FR-001), from platform/member-api, never from the identity feature; the partner follows from it
 * (`partnerOf` in labels.ts). `person` is undefined both while the member loads and for a member without a person;
 * `isLoading` tells them apart. A failed read counts as a member without a person, so the section still works.
 */
export function useMe() {
  const member = useCurrentMember();
  return {
    person: member.data?.person ?? undefined,
    isLoading: member.isPending,
  };
}
