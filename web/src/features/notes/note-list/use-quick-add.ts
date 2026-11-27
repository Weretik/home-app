import { useRef, useState, type FormEvent } from 'react';
import { quickAddPrompt } from '../data/labels';
import type { NoteKind, NotePerson } from '../data/notes-api';
import { quickNote, titleError } from '../note-dialog/note-form';
import { describeWriteError } from '../writes/failures-api';
import { useIdempotencyKey, useSaveNote } from '../writes/note-writes-api';

interface QuickAddOptions {
  /** The tab's person; undefined in «Усі», where a person toggle picks one. */
  about: NotePerson | undefined;
  /** The signed-in person, so the prompt says «Що я люблю?». */
  me: NotePerson | undefined;
  /** The kind it starts with: the filtered one, else «Любить». */
  startKind: NoteKind;
}

/**
 * The quick add's state (FR-003): the kind, the person in «Усі», the text and its check. Enter adds a note about the
 * person with that kind and no category; the field clears and keeps focus. One Idempotency-Key per text.
 */
export function useQuickAdd({ about, me, startKind }: QuickAddOptions) {
  const [kind, setKind] = useState(startKind);
  const [shownStart, setShownStart] = useState(startKind);
  // The kind filter changed: start from it.
  if (startKind !== shownStart) {
    setShownStart(startKind);
    setKind(startKind);
  }
  const [chosen, setChosen] = useState<NotePerson>(me ?? 'Vitaliy');
  const person = about ?? chosen;
  const [text, setText] = useState('');
  const [check, setCheck] = useState<string>();
  const input = useRef<HTMLInputElement>(null);
  const save = useSaveNote();
  const idempotency = useIdempotencyKey();
  const failure = save.error
    ? describeWriteError(save.error, 'додати')
    : undefined;

  const submit = (event: FormEvent) => {
    event.preventDefault();
    const error = titleError(text);
    setCheck(text.trim() === '' ? undefined : error);
    if (error || save.isPending) return;
    const body = quickNote({ about: person, kind, title: text });
    save.mutate(
      { body, idempotencyKey: idempotency.keyFor(body) },
      {
        onSuccess: () => {
          idempotency.renew();
          setText('');
          input.current?.focus();
        },
        onError: (e) => {
          if (describeWriteError(e).renewKey) idempotency.renew();
        },
      },
    );
  };

  return {
    kind,
    setKind,
    person,
    setPerson: setChosen,
    text,
    setText: (value: string) => {
      setText(value);
      setCheck(undefined);
      if (save.isError) save.reset();
    },
    check,
    input,
    prompt: quickAddPrompt(kind, person, me),
    error: failure?.message,
    isPending: save.isPending,
    submit,
    /** The draft the full dialog starts with. */
    draft: () => ({ about: person, kind, title: text.trim() }),
  };
}
