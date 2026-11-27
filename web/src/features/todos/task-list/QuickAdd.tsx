import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import EditNoteOutlinedIcon from '@mui/icons-material/EditNoteOutlined';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import TextField from '@mui/material/TextField';
import Tooltip from '@mui/material/Tooltip';
import { useRef, useState, type FormEvent } from 'react';
import { isOnline } from '../../../platform/offline/status/network-status';
import type { Assignee } from '../data/todos-api';
import { quickTask, titleError } from '../task-dialog/task-form';
import { describeWriteError } from '../writes/failures-api';
import { useAddTask, useIdempotencyKey } from '../writes/task-writes-api';

/**
 * «Нова справа…» on top of the list (FR-003): Enter or «Додати» adds a task for `assignee`, Звичайна, without a date;
 * the field clears and keeps focus. The pencil opens the full form with the typed title. Offline the add is queued
 * (web ADR 0022): the field clears at once and says when it will be added.
 */
export function QuickAdd({
  assignee,
  onOpenForm,
}: {
  assignee: Assignee;
  onOpenForm: (title: string) => void;
}) {
  const [title, setTitle] = useState('');
  const [check, setCheck] = useState<string>();
  const input = useRef<HTMLInputElement>(null);
  const save = useAddTask();
  const [queued, setQueued] = useState(false);
  const idempotency = useIdempotencyKey();
  const failure = save.error
    ? describeWriteError(save.error, 'додати')
    : undefined;

  const submit = (event: FormEvent) => {
    event.preventDefault();
    const error = titleError(title);
    setCheck(title.trim() === '' ? undefined : error);
    if (error || save.isSaving) return;
    const body = quickTask(title, assignee);
    const offline = !isOnline();
    setQueued(offline);
    save.mutate(
      { body, idempotencyKey: idempotency.keyFor(body) },
      {
        onSuccess: () => {
          if (offline) return;
          idempotency.renew();
          setTitle('');
          input.current?.focus();
        },
        onError: (e) => {
          if (describeWriteError(e).renewKey) idempotency.renew();
        },
      },
    );
    if (offline) {
      // Queued (FR-006): the field clears at once; the next add of the same title is a new task.
      idempotency.renew();
      setTitle('');
      input.current?.focus();
    }
  };

  return (
    <Box
      component="form"
      onSubmit={submit}
      sx={{ p: 2, display: 'grid', gap: 1 }}
    >
      <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1 }}>
        <TextField
          inputRef={input}
          size="small"
          fullWidth
          placeholder="Нова справа…"
          value={title}
          onChange={(event) => {
            setTitle(event.target.value);
            setCheck(undefined);
            if (save.isError) save.reset();
          }}
          error={Boolean(check)}
          helperText={check}
          slotProps={{
            htmlInput: { 'aria-label': 'Нова справа', enterKeyHint: 'done' },
          }}
        />
        <Tooltip title="Додати">
          <span>
            <IconButton
              type="submit"
              color="primary"
              aria-label="Додати"
              disabled={save.isSaving}
            >
              <AddOutlinedIcon />
            </IconButton>
          </span>
        </Tooltip>
        <Tooltip title="Детальна форма">
          <IconButton
            aria-label="Детальна форма"
            onClick={() => onOpenForm(title.trim())}
          >
            <EditNoteOutlinedIcon />
          </IconButton>
        </Tooltip>
      </Box>
      {failure && <Alert severity="error">{failure.message}</Alert>}
      {queued && save.isPaused && (
        <Alert severity="info" onClose={() => setQueued(false)}>
          Додасться, коли з’явиться мережа
        </Alert>
      )}
    </Box>
  );
}
