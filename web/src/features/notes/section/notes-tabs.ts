import { personAbout, personNames } from '../data/labels';
import type { NotePerson } from '../data/notes-api';

export const sectionPath = '/notes';
export const settingsPath = `${sectionPath}/settings`;

/** The tab in the URL (`?about=`): relative to the signed-in member, or the people by name for a member without one. */
export type NoteTab = 'me' | 'partner' | 'all' | 'vitaliy' | 'maria';

export interface TabOption {
  value: NoteTab;
  label: string;
  /** Whom the tab's notes are about; undefined for «Усі». */
  about?: NotePerson;
}

const byName: Record<NotePerson, NoteTab> = {
  Vitaliy: 'vitaliy',
  Maria: 'maria',
};

const all: TabOption = { value: 'all', label: 'Усі' };

/**
 * The tabs (FR-001, web ADR 0017): «Про мене», «Про Машу» (the partner by name), «Усі»; a member without a person
 * sees «Віталій», «Маша», «Усі».
 */
export function tabOptions(person: NotePerson | undefined): TabOption[] {
  if (!person)
    return [
      { value: 'vitaliy', label: personNames.Vitaliy, about: 'Vitaliy' },
      { value: 'maria', label: personNames.Maria, about: 'Maria' },
      all,
    ];
  const partner: NotePerson = person === 'Vitaliy' ? 'Maria' : 'Vitaliy';
  return [
    { value: 'me', label: 'Про мене', about: person },
    {
      value: 'partner',
      label: `Про ${personAbout[partner]}`,
      about: partner,
    },
    all,
  ];
}

/**
 * The open tab: the one in the URL, a person's name turned into «Про мене» or the partner's tab for a member with a
 * person (so a link with `?about=maria` opens the same notes for both), and otherwise the default: «Про мене», or «Усі»
 * without a person.
 */
export function resolveTab(
  raw: string | null,
  person: NotePerson | undefined,
): TabOption {
  const options = tabOptions(person);
  const found = options.find((option) => option.value === raw);
  if (found) return found;
  const named = options.find(
    (option) => option.about && byName[option.about] === raw,
  );
  if (named) return named;
  return person ? options[0]! : all;
}
