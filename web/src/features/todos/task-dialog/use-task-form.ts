import { useState } from 'react';
import type { Assignee, TodoTaskItem } from '../data/todos-api';
import {
  describeWriteError,
  firstMessages,
  isChangedMeanwhile,
} from '../writes/failures-api';
import { useIdempotencyKey, useSaveTask } from '../writes/task-writes-api';
import {
  isValid,
  taskFormValues,
  toTaskCreate,
  toTaskUpdate,
  validateTask,
  type FieldErrors,
  type TaskFormValues,
} from './task-form';

interface TaskFormOptions {
  /** The task to change as it is now (useTask); undefined adds a new one. */
  current?: TodoTaskItem;
  /** Reads the task again, after someone else changed it. */
  refetch: () => Promise<{ data?: TodoTaskItem }>;
  /** The assignee and the title a new task starts with. */
  assignee: Assignee;
  initialTitle?: string;
  /** The titles of the steps a new task gets with its create. */
  newSteps: readonly string[];
  onSaved: () => void;
}

/**
 * The state of the task dialog: the values, the field messages (the form's checks, then the API's) and the submit. A
 * create sends the steps and an Idempotency-Key; an edit sends the version of the task as it is now (steps saved at once
 * change it). When the task was changed elsewhere, the form reloads it (SC-005).
 */
export function useTaskForm({
  current,
  refetch,
  assignee,
  initialTitle = '',
  newSteps,
  onSaved,
}: TaskFormOptions) {
  const [values, setValues] = useState<TaskFormValues>(() =>
    current
      ? taskFormValues(current)
      : { ...taskFormValues(undefined, assignee), title: initialTitle },
  );
  const [checked, setChecked] = useState<FieldErrors>({});
  const save = useSaveTask();
  const idempotency = useIdempotencyKey();
  const failure = save.error ? describeWriteError(save.error) : undefined;

  const set = <K extends keyof TaskFormValues>(
    name: K,
    value: TaskFormValues[K],
  ) => {
    setValues((current) => ({ ...current, [name]: value }));
    setChecked((current) => ({ ...current, [name]: undefined }));
  };

  const reload = async () => {
    const fresh = await refetch();
    if (fresh.data) setValues(taskFormValues(fresh.data));
  };

  const submit = () => {
    const errors = validateTask(values);
    setChecked(errors);
    if (!isValid(errors) || save.isPending) return;
    if (current) {
      const edit = { id: current.id, version: current.version };
      save.mutate(
        { body: toTaskUpdate(values), edit },
        {
          onSuccess: onSaved,
          onError: (error) => {
            if (isChangedMeanwhile(error)) void reload();
          },
        },
      );
      return;
    }
    const body = toTaskCreate(values, newSteps);
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
