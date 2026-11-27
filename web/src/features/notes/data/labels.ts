import { locale } from '../../../platform/client/locale';
import type { NoteItem, NoteKind, NotePerson } from './notes-api';

/** The kinds in the order the screens list them, «Любить» first (FR-003). */
export const kinds: readonly NoteKind[] = ['Likes', 'Dislikes', 'Fact'];

export const kindLabels: Record<NoteKind, string> = {
  Likes: 'Любить',
  Dislikes: 'Не любить',
  Fact: 'Факт',
};

export const people: readonly NotePerson[] = ['Vitaliy', 'Maria'];

/** The other person of the two. */
export const partnerOf: Record<NotePerson, NotePerson> = {
  Vitaliy: 'Maria',
  Maria: 'Vitaliy',
};

/** «Віталій», «Маша». */
export const personNames: Record<NotePerson, string> = {
  Vitaliy: 'Віталій',
  Maria: 'Маша',
};

/** The accusative after «про»: «Про Віталія», «Про Машу». */
export const personAbout: Record<NotePerson, string> = {
  Vitaliy: 'Віталія',
  Maria: 'Машу',
};

/** The avatar letter of a person. */
export const personInitials: Record<NotePerson, string> = {
  Vitaliy: 'В',
  Maria: 'М',
};

/** «змінив Віталій», «змінила Маша». */
const editedBy: Record<NotePerson, string> = {
  Vitaliy: 'змінив Віталій',
  Maria: 'змінила Маша',
};

/**
 * What the quick add asks (FR-003): «Що любить Маша?», or «Що я люблю?» when the note is about the signed-in person.
 */
export function quickAddPrompt(
  kind: NoteKind,
  about: NotePerson,
  me: NotePerson | undefined,
): string {
  if (about === me)
    return {
      Likes: 'Що я люблю?',
      Dislikes: 'Що я не люблю?',
      Fact: 'Що про мене варто знати?',
    }[kind];
  const name = personNames[about];
  return {
    Likes: `Що любить ${name}?`,
    Dislikes: `Що не любить ${name}?`,
    Fact: `Що варто знати про ${personAbout[about]}?`,
  }[kind];
}

const shortDate = new Intl.DateTimeFormat(locale, {
  day: '2-digit',
  month: '2-digit',
  timeZone: 'Europe/Kyiv',
});

/** A card's footer (FR-004): «Маша · 27.11», plus «змінив Віталій» when someone else edited it last. */
export function noteFooter(
  note: Pick<NoteItem, 'createdBy' | 'updatedBy' | 'createdAtUtc'>,
): string {
  const parts = [shortDate.format(new Date(note.createdAtUtc))];
  if (note.createdBy) parts.unshift(personNames[note.createdBy]);
  if (note.updatedBy && note.updatedBy !== note.createdBy)
    parts.push(editedBy[note.updatedBy]);
  return parts.join(' · ');
}

/** «1 нотатка», «3 нотатки», «12 нотаток». */
export function formatNoteCount(count: number): string {
  const forms = { one: 'нотатка', few: 'нотатки', many: 'нотаток' } as const;
  const rule = new Intl.PluralRules(locale).select(count);
  return `${count} ${forms[rule as keyof typeof forms] ?? forms.many}`;
}
