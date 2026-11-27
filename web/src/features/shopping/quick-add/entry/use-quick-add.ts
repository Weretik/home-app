import { useState } from 'react';
import {
  useAddItems,
  useIdempotencyKey,
  useUndoAdd,
  type AddItemsResult,
} from '../../data/item-writes-api';
import { describeWriteError } from '../../data/write-failure-api';
import { useMe } from '../../section/use-me';

export interface AddOptions {
  /** Puts every item in this department instead of matching (a ⭐ chip knows its department). */
  departmentId?: string;
  urgent?: boolean;
}

/** The snackbar text after an add: what was added, brought back or only got a new quantity. */
export function addedMessage(result: AddItemsResult): string {
  const names = [...result.added, ...result.restored, ...result.updated].map(
    (item) => item.name,
  );
  if (names.length === 0) return 'Нічого не додано';
  return result.added.length + result.restored.length === 0
    ? `Вже в списку: ${names.join(', ')}`
    : `Додано: ${names.join(', ')}`;
}

/**
 * The quick add and its «Скасувати» (R-1, R-2, R-8): one add at a time with an Idempotency-Key per attempt, «Я» as
 * the author; the last result stays for the snackbar until it closes or is undone.
 */
export function useQuickAdd() {
  const [me] = useMe();
  const add = useAddItems();
  const undo = useUndoAdd();
  const idempotency = useIdempotencyKey();
  const [last, setLast] = useState<AddItemsResult | null>(null);
  const [error, setError] = useState<string | null>(null);

  const submit = (
    text: string,
    options: AddOptions = {},
    onDone?: () => void,
  ) => {
    const body = {
      text,
      addedBy: me,
      departmentId: options.departmentId ?? null,
      urgent: options.urgent ?? false,
    };
    setError(null);
    add.mutate(
      { body, idempotencyKey: idempotency.keyFor(body) },
      {
        onSuccess: (result) => {
          idempotency.renew();
          setLast(result);
          onDone?.();
        },
        onError: (failure) => {
          const described = describeWriteError(failure, 'додати');
          if (described.renewKey) idempotency.renew();
          setError(
            Object.values(described.fieldErrors)[0]?.[0] ?? described.message,
          );
        },
      },
    );
  };

  const undoLast = () => {
    if (!last) return;
    const added = last.added;
    setLast(null);
    undo.mutate(added, {
      onError: (failure) =>
        setError(describeWriteError(failure, 'скасувати').message),
    });
  };

  return {
    submit,
    isPending: add.isPending,
    last,
    dismiss: () => setLast(null),
    undoLast,
    error,
    clearError: () => setError(null),
  };
}

export type QuickAddController = ReturnType<typeof useQuickAdd>;
