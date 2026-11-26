import type { UseMutationResult } from '@tanstack/react-query';
import { useState } from 'react';
import { describeWriteError } from '../data/write-errors-api';
import {
  useIdempotencyKey,
  type EditedEntry,
  type SaveInput,
} from '../data/writes-api';
import { isValid, type FieldErrors } from './field-checks';

interface EditFormOptions<Values, Body> {
  initial: Values;
  /** Field messages of the values; undefined messages when they are fine. */
  validate: (values: Values) => FieldErrors;
  toBody: (values: Values) => Body;
  /** The item being changed; undefined for a new one. */
  edit?: EditedEntry;
  save: UseMutationResult<unknown, Error, SaveInput<Body>>;
  onSaved: () => void;
}

/**
 * The state of an add / edit form (web ADR 0007): its values, the field messages (the form's own checks, then the
 * API's), and the submit, which sends a create with the form's Idempotency-Key or an edit with the item's version.
 * A failed save keeps the typed values.
 */
export function useEditForm<Values extends object, Body>({
  initial,
  validate,
  toBody,
  edit,
  save,
  onSaved,
}: EditFormOptions<Values, Body>) {
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
    setChecked({});
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

  const own = Object.fromEntries(
    Object.entries(checked).filter(([, message]) => message),
  );
  return {
    values,
    set,
    errors: { ...serverErrors, ...own } as FieldErrors,
    message: failure?.message,
    submit,
    isPending: save.isPending,
  };
}
