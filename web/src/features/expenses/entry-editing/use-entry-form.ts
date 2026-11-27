import { useState } from 'react';
import { isOnline } from '../../../platform/offline/status/network-status';
import { describeWriteError } from '../write-data/write-errors-api';
import {
  useIdempotencyKey,
  type EditedEntry,
  type Saver,
} from '../write-data/write-hooks-api';
import { isValid, type FieldErrors } from './entry-fields';

interface EntryFormOptions<Values, Body> {
  initial: Values;
  /** Field messages of the values; an empty object or undefined messages when they are fine. */
  validate: (values: Values) => FieldErrors;
  toBody: (values: Values) => Body;
  /** The entry being changed; undefined for a new one. */
  edit?: EditedEntry;
  save: Saver<Body>;
  onSaved: () => void;
  /** Set when `save` is a queued write: called at once when the write is made offline and waits for the network. */
  onQueued?: () => void;
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
  onQueued,
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
    const queued = onQueued !== undefined && !isOnline();
    save.mutate(
      { body, edit, idempotencyKey: idempotency.keyFor(body) },
      {
        onSuccess: onSaved,
        onError: (error) => {
          if (describeWriteError(error).renewKey) idempotency.renew();
        },
      },
    );
    if (queued) onQueued();
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
