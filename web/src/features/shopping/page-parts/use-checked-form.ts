import { useState } from 'react';
import {
  describeWriteError,
  type WriteFailure,
} from '../data/write-failure-api';

export type FormErrors<V> = Partial<Record<keyof V, string>>;

/**
 * The state of a form that checks itself before a save (design.md, «Editing entries»): the typed values, the
 * messages of its own checks, and the failed save with the API's field errors. `submit` runs the checks and calls
 * `send` only when they pass; `fail` turns a failed save into the form's messages and returns it.
 */
export function useCheckedForm<V extends object>(
  initial: () => V,
  check: (values: V) => FormErrors<V>,
) {
  const [values, setValues] = useState(initial);
  const [errors, setErrors] = useState<FormErrors<V>>({});
  const [failure, setFailure] = useState<WriteFailure>();

  const submit = (
    send: (values: V, fail: (error: Error) => WriteFailure) => void,
  ) => {
    const found = check(values);
    setErrors(found);
    if (Object.keys(found).length > 0) return;
    setFailure(undefined);
    send(values, (error) => {
      const described = describeWriteError(error);
      setFailure(described);
      return described;
    });
  };

  /** The message under a field: its own check first, then the API's. */
  const errorOf = (field: keyof V & string): string | undefined =>
    errors[field] ?? failure?.fieldErrors[field]?.[0];

  return { values, setValues, submit, errorOf, message: failure?.message };
}
