import { useState } from 'react';
import type { TodoTaskItem } from '../data/todos-api';
import type { ResultMessage } from '../feedback/ResultSnackbar';
import { describeWriteError } from '../writes/failures-api';
import { useMoveTask, useRestoreStatus } from '../writes/status-api';

/**
 * The one-tap done of the active list (FR-004): the row leaves at once, «Готово · Скасувати» shows, and «Скасувати»
 * puts back the status the task had. A failed write brings the row back and says why.
 */
export function useDoneTick() {
  const move = useMoveTask();
  const restore = useRestoreStatus();
  const [message, setMessage] = useState<ResultMessage | null>(null);

  const fail = (action: string) => (error: Error) =>
    setMessage({
      text: describeWriteError(error, action).message,
      severity: 'error',
    });

  const complete = (task: TodoTaskItem) => {
    const moved = move
      .mutateAsync({ id: task.id, version: task.version, status: 'Done' })
      .then(
        () => true,
        (error: Error) => {
          fail('позначити справу')(error);
          return false;
        },
      );
    // «Скасувати» waits for the tick: undoing before it is saved would read the old version and race the tick. A failed
    // tick has nothing to undo (its row is already back).
    const undo = () =>
      void moved.then((done) =>
        done
          ? restore
              .mutateAsync({ id: task.id, status: task.status })
              .then(() => undefined, fail('повернути справу'))
          : undefined,
      );
    setMessage({ text: `Готово: ${task.title}`, severity: 'success', undo });
  };

  return { complete, message, dismiss: () => setMessage(null) };
}
