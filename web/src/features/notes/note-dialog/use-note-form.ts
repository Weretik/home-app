import { useState } from 'react';
import type { NoteItem } from '../data/notes-api';
import {
  describeWriteError,
  firstMessages,
  isChangedMeanwhile,
  isNoteGone,
} from '../writes/failures-api';
import { useIdempotencyKey, useSaveNote } from '../writes/note-writes-api';
import {
  isValid,
  noteFormValues,
  toNoteRequest,
  validateNote,
  type FieldErrors,
  type NoteDraft,
  type NoteFormValues,
} from './note-form';

interface NoteFormOptions {
  /** The note to change as the list holds it now; undefined adds a new one. */
  current?: NoteItem;
  /** What a new note starts with. */
  draft?: NoteDraft;
  onSaved: () => void;
  /** The note was deleted elsewhere (404): the dialog closes and the page says so. */
  onGone: () => void;
}

/**
 * The state of the note dialog: the values, the field messages (the form's checks, then the API's) and the submit. A
 * create sends an Idempotency-Key; an edit sends the version in If-Match. After a 409 the list reloads and the form
 * takes the note as it is now (FR-005).
 */
export function useNoteForm({
  current,
  draft,
  onSaved,
  onGone,
}: NoteFormOptions) {
  const [values, setValues] = useState(() => noteFormValues(current, draft));
  const [checked, setChecked] = useState<FieldErrors>({});
  // The version a 409 answered to; when the list brings a newer one, the form shows it.
  const [staleVersion, setStaleVersion] = useState<number>();
  if (
    current &&
    staleVersion !== undefined &&
    current.version !== staleVersion
  ) {
    setStaleVersion(undefined);
    setValues(noteFormValues(current));
  }
  const save = useSaveNote();
  const idempotency = useIdempotencyKey();
  const failure = save.error ? describeWriteError(save.error) : undefined;

  const set = <K extends keyof NoteFormValues>(
    name: K,
    value: NoteFormValues[K],
  ) => {
    setValues((old) => ({ ...old, [name]: value }));
    setChecked((old) => ({ ...old, [name]: undefined }));
  };

  const submit = () => {
    const errors = validateNote(values);
    setChecked(errors);
    if (!isValid(errors) || save.isPending) return;
    const body = toNoteRequest(values);
    if (current) {
      save.mutate(
        { body, edit: { id: current.id, version: current.version } },
        {
          onSuccess: onSaved,
          onError: (error) => {
            if (isNoteGone(error)) onGone();
            else if (isChangedMeanwhile(error))
              setStaleVersion(current.version);
          },
        },
      );
      return;
    }
    save.mutate(
      { body, idempotencyKey: idempotency.keyFor(body) },
      {
        onSuccess: onSaved,
        onError: (error) => {
          if (describeWriteError(error).renewKey) idempotency.renew();
        },
      },
    );
  };

  const shown = Object.fromEntries(
    Object.entries(checked).filter(([, text]) => text),
  );
  return {
    values,
    set,
    errors: { ...firstMessages(failure), ...shown },
    message: failure?.message,
    submit,
    isPending: save.isPending,
  };
}
