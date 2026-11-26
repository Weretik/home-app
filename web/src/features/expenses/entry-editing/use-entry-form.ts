import type { UseMutationResult } from '@tanstack/react-query';
import { useState } from 'react';
import { describeWriteError } from '../write-data/write-errors-api';
import {
  useIdempotencyKey,
  type EditedEntry,
  type SaveInput,
} from '../write-data/write-hooks-api';
import { isValid, type FieldErrors } from './entry-fields';

interface EntryFormOptions<Values, Body> {
  initial: Values;
  /** Field messages of the values; an empty object or undefined messages when they are fine. */
  validate: (values: Values) => FieldErrors;
  toBody: (values: Values) => Body;
  /** The entry being changed; undefined for a new one. */
  edit?: EditedEntry;
  save: UseMutationResult<unknown, Error, SaveInput<Body>>;
  onSaved: () => void;
}

/**
 * The state of an add / edit form: its values, the field messages (the form's own checks, then the API's), and the
 * submit, which sends a create with the form's Idempotency-Key or an edit with the entry's version.
 */
export function useEntryForm<Values extends object, Body>({
  initial,
  validate,
  toBody,
  edit,
  save,
  onSaved,
}: EntryFormOptions<Values, Body>) {
  const [values, setValues] = useState(initial);
  const [checked, setChecked] = useState<FieldErrors>({});
  const idempotency = useIdempotencyKey();
  const failure = save.error ? describeWriteError(save.error) : undefined;

  const serverErrors: FieldErrors = Object.fromEntries(
    Object.entries(failure?.fieldErrors ?? {}).map(([name, messages]) => [
      name,
      messages[0],
    ]),
  );

  const set = <K extends keyof Values>(name: K, value: Values[K]) => {
    setValues((current) => ({ ...current, [name]: value }));
    setChecked((current) => ({ ...current, [name]: undefined }));
  };

  const submit = () => {
    const errors = validate(values);
    setChecked(errors);
    if (!isValid(errors) || save.isPending) return;
    const body = toBody(values);
    save.mutate(
      { body, edit, idempotencyKey: idempotency.keyFor(body) },
      {
        onSuccess: onSaved,
        onError: (error) => {
          if (describeWriteError(error).renewKey) idempotency.renew();
        },
      },
    );
  };

  return {
    values,
    set,
    errors: { ...serverErrors, ...stripEmpty(checked) },
    message: failure?.message,
    submit,
    isPending: save.isPending,
  };
}

const stripEmpty = (errors: FieldErrors): FieldErrors =>
  Object.fromEntries(Object.entries(errors).filter(([, message]) => message));
