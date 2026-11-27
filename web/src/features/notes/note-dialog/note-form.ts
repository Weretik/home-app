import type { NoteItem, NoteKind, NotePerson } from '../data/notes-api';
import type { NoteRequest } from '../writes/note-writes-api';

/** The API limits (R-003…R-005). */
export const MAX_TITLE = 200;
export const MAX_DETAILS = 2000;
export const MAX_PLACE = 200;

/** What the note form edits; '' is «none» for the category. */
export interface NoteFormValues {
  about: NotePerson;
  kind: NoteKind;
  title: string;
  categoryId: string;
  place: string;
  details: string;
  isImportant: boolean;
}

/** What a new note starts with: the tab's person, the quick add's kind and typed text. */
export type NoteDraft = Pick<NoteFormValues, 'about' | 'kind' | 'title'>;

/** Field messages by the contract's camelCase field name. */
export type FieldErrors = Record<string, string | undefined>;

/** The form of an existing note, or of a new one from its draft. */
export function noteFormValues(
  note: NoteItem | undefined,
  draft?: NoteDraft,
): NoteFormValues {
  if (!note)
    return {
      about: draft?.about ?? 'Vitaliy',
      kind: draft?.kind ?? 'Likes',
      title: draft?.title ?? '',
      categoryId: '',
      place: '',
      details: '',
      isImportant: false,
    };
  return {
    about: note.about,
    kind: note.kind,
    title: note.title,
    categoryId: note.categoryId ?? '',
    place: note.place ?? '',
    details: note.details ?? '',
    isImportant: note.isImportant,
  };
}

export function titleError(title: string): string | undefined {
  const trimmed = title.trim();
  if (trimmed === '') return 'Введіть, що саме';
  if (trimmed.length > MAX_TITLE) return `Не довше за ${MAX_TITLE} символів`;
  return undefined;
}

/** The form's own checks, before anything is sent (FR-005). */
export function validateNote(values: NoteFormValues): FieldErrors {
  const errors: FieldErrors = { title: titleError(values.title) };
  if (values.place.trim().length > MAX_PLACE)
    errors.place = `Не довше за ${MAX_PLACE} символів`;
  if (values.details.trim().length > MAX_DETAILS)
    errors.details = `Опис не довший за ${MAX_DETAILS} символів`;
  return errors;
}

export const isValid = (errors: FieldErrors) =>
  Object.values(errors).every((message) => !message);

const orNull = (text: string) => {
  const trimmed = text.trim();
  return trimmed === '' ? null : trimmed;
};

/** The request of the values: trimmed text, '' sent as null. */
export function toNoteRequest(values: NoteFormValues): NoteRequest {
  return {
    about: values.about,
    kind: values.kind,
    title: values.title.trim(),
    details: orNull(values.details),
    place: orNull(values.place),
    categoryId: values.categoryId === '' ? null : values.categoryId,
    isImportant: values.isImportant,
  };
}

/** The create request of a quick-added text (FR-003): the tab's person, the chosen kind, no category. */
export const quickNote = (draft: NoteDraft): NoteRequest =>
  toNoteRequest(noteFormValues(undefined, draft));
