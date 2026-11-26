import { useState } from 'react';
import type { TaskStepItem, TodoTaskItem } from '../data/todos-api';
import { describeWriteError } from '../writes/failures-api';
import { useStepWrites } from '../writes/steps-api';

/** The most steps a task holds (R-009). */
export const MAX_STEPS = 50;

/**
 * The steps of the open task (FR-005). An existing task saves each change at once through the step endpoints; a new
 * task keeps them here and sends their titles with its create. A tick shows at once while it is being saved.
 */
export function useSteps(task: TodoTaskItem | undefined) {
  const [local, setLocal] = useState<TaskStepItem[]>([]);
  const writes = useStepWrites(task?.id ?? '');
  const { add, update, remove } = writes;

  const saved = task?.steps ?? local;
  const ticking = update.isPending ? update.variables : undefined;
  const steps = saved
    .filter((step) => !(remove.isPending && remove.variables === step.id))
    .map((step) =>
      ticking?.stepId === step.id
        ? { ...step, title: ticking.title, isDone: ticking.isDone }
        : step,
    );
  const failed = [add, update, remove].find((m) => m.error)?.error;

  return {
    steps,
    isNew: !task,
    isFull: steps.length >= MAX_STEPS,
    isPending: add.isPending || update.isPending || remove.isPending,
    isAdding: add.isPending,
    message: failed
      ? describeWriteError(failed, 'сохранить шаг').message
      : undefined,
    add: (title: string, onDone: () => void) => {
      if (!task) {
        setLocal((current) => [
          ...current,
          { id: crypto.randomUUID(), title, isDone: false },
        ]);
        onDone();
        return;
      }
      add.mutate(title, { onSuccess: onDone });
    },
    rename: (step: TaskStepItem, title: string) => {
      if (!task)
        setLocal((current) =>
          current.map((s) => (s.id === step.id ? { ...s, title } : s)),
        );
      else update.mutate({ stepId: step.id, title, isDone: step.isDone });
    },
    toggle: (step: TaskStepItem) => {
      if (task)
        update.mutate({
          stepId: step.id,
          title: step.title,
          isDone: !step.isDone,
        });
    },
    remove: (step: TaskStepItem) => {
      if (!task) setLocal((current) => current.filter((s) => s.id !== step.id));
      else remove.mutate(step.id);
    },
  };
}

export type StepsState = ReturnType<typeof useSteps>;
