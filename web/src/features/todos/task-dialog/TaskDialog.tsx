import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import type { Theme } from '@mui/material/styles';
import useMediaQuery from '@mui/material/useMediaQuery';
import { useId, useState, type FormEvent } from 'react';
import { useTask, type Assignee, type TodoTaskItem } from '../data/todos-api';
import { ConfirmDeleteDialog } from '../feedback/ConfirmDeleteDialog';
import { StepsEditor } from '../task-steps/StepsEditor';
import { useSteps } from '../task-steps/use-steps';
import { describeWriteError } from '../writes/failures-api';
import { useDeleteTask } from '../writes/task-writes-api';
import { TaskFields } from './TaskFields';
import { useTaskForm } from './use-task-form';

interface TaskDialogProps {
  /** The task to change; undefined adds a new one. */
  item?: TodoTaskItem;
  /** Whom a new task is for, and the title it starts with. */
  assignee: Assignee;
  initialTitle?: string;
  onClose: () => void;
}

/**
 * A task in a dialog (FR-005), full screen on a phone and 600 px wide on a computer: the fields, the steps, and
 * «Удалить» (asks first), «Отмена», «Сохранить». Closing waits for a pending save.
 */
export function TaskDialog({
  item,
  assignee,
  initialTitle,
  onClose,
}: TaskDialogProps) {
  const titleId = useId();
  const isPhone = useMediaQuery((theme: Theme) => theme.breakpoints.down('sm'));
  const task = useTask(item);
  const current = item ? (task.data ?? item) : undefined;
  const steps = useSteps(current);
  const form = useTaskForm({
    current,
    refetch: task.refetch,
    assignee,
    initialTitle,
    newSteps: steps.steps.map((step) => step.title),
    onSaved: onClose,
  });
  const [confirming, setConfirming] = useState(false);
  const remove = useDeleteTask();
  const busy = form.isPending || steps.isPending || remove.isPending;

  const submit = (event: FormEvent) => {
    event.preventDefault();
    if (!steps.isPending) form.submit();
  };

  return (
    <Dialog
      open
      fullWidth
      maxWidth="sm"
      fullScreen={isPhone}
      onClose={busy ? undefined : onClose}
      aria-labelledby={titleId}
    >
      <Box
        component="form"
        noValidate
        onSubmit={submit}
        sx={{ display: 'flex', flexDirection: 'column', minHeight: 0, flex: 1 }}
      >
        <DialogTitle id={titleId}>
          {item ? 'Задача' : 'Новая задача'}
        </DialogTitle>
        <DialogContent
          sx={{
            display: 'grid',
            gap: 2,
            alignContent: 'start',
            '.MuiDialogTitle-root + &': { pt: 1 },
          }}
        >
          {form.message && <Alert severity="error">{form.message}</Alert>}
          <TaskFields
            values={form.values}
            errors={form.errors}
            set={form.set}
          />
          <StepsEditor state={steps} />
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2, gap: 1 }}>
          {item && (
            <Button
              color="error"
              onClick={() => setConfirming(true)}
              disabled={busy}
              sx={{ mr: 'auto' }}
            >
              Удалить
            </Button>
          )}
          <Button onClick={onClose} disabled={form.isPending}>
            Отмена
          </Button>
          <Button type="submit" variant="contained" disabled={busy}>
            {form.isPending ? 'Сохраняю…' : 'Сохранить'}
          </Button>
        </DialogActions>
      </Box>
      {confirming && item && (
        <ConfirmDeleteDialog
          title="Удалить задачу?"
          summary={item.title}
          warning="Задача и её шаги удалятся насовсем. Если она просто не нужна, поставьте статус «Не актуально»."
          error={
            remove.error
              ? describeWriteError(remove.error, 'удалить').message
              : undefined
          }
          isPending={remove.isPending}
          onConfirm={() =>
            remove.mutate(
              { id: item.id, version: (current ?? item).version },
              { onSuccess: onClose },
            )
          }
          onClose={() => {
            remove.reset();
            setConfirming(false);
          }}
        />
      )}
    </Dialog>
  );
}
