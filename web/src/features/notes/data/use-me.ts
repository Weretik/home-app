import { useCurrentMember } from '../../../platform/member-api';
import type { NotePerson } from './notes-api';

export const partnerOf: Record<NotePerson, NotePerson> = {
  Vitaliy: 'Maria',
  Maria: 'Vitaliy',
};

/**
 * The signed-in person and the partner (FR-001), from platform/member-api, never from the identity feature.
 * `person` is undefined both while the member loads and for a member without a person; `isLoading` tells them apart.
 * A failed read counts as a member without a person, so the section still works.
 */
export function useMe() {
  const member = useCurrentMember();
  const person = member.data?.person ?? undefined;
  return {
    person,
    partner: person ? partnerOf[person] : undefined,
    isLoading: member.isPending,
  };
}
