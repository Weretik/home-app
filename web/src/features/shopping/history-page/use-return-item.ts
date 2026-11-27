import { useState } from 'react';
import { isOnline } from '../../../platform/offline/status/network-status';
import { useAddItems, useIdempotencyKey } from '../data/item-writes-api';
import type { ShoppingItem } from '../data/shopping-api';
import { describeWriteError } from '../data/write-failure-api';
import { useMe } from '../section/use-me';

/**
 * «Повернути до списку» (R-10): adds the bought item again with its quantity and department, as «Я», with an
 * Idempotency-Key per attempt. `returning` is the id whose button waits; `notice` is the snackbar text.
 */
export function useReturnItem() {
  const add = useAddItems();
  const idempotency = useIdempotencyKey();
  const [me] = useMe();
  const [returning, setReturning] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);

  const returnItem = (item: ShoppingItem) => {
    const text = item.quantity ? `${item.name} ${item.quantity}` : item.name;
    const body = {
      text,
      addedBy: me,
      departmentId: item.departmentId,
      urgent: false,
    };
    if (!isOnline()) {
      // Queued (web ADR 0022): nothing to wait for on this button.
      setNotice(`Повернеться до списку, коли з’явиться мережа: ${item.name}`);
      add.mutate({ body, idempotencyKey: idempotency.keyFor(body) });
      idempotency.renew();
      return;
    }
    setReturning(item.id);
    add.mutate(
      { body, idempotencyKey: idempotency.keyFor(body) },
      {
        onSuccess: () => {
          idempotency.renew();
          setNotice(`Повернули до списку: ${item.name}`);
        },
        onError: (e) => setNotice(describeWriteError(e, 'повернути').message),
        onSettled: () => setReturning(null),
      },
    );
  };

  return { returnItem, returning, notice, clearNotice: () => setNotice(null) };
}
