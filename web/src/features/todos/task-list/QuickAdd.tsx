import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import EditNoteOutlinedIcon from '@mui/icons-material/EditNoteOutlined';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import TextField from '@mui/material/TextField';
import Tooltip from '@mui/material/Tooltip';
import { useRef, useState, type FormEvent } from 'react';
import type { Assignee } from '../data/todos-api';
import { quickTask, titleError } from '../task-dialog/task-form';
import { describeWriteError } from '../writes/failures-api';
import { useIdempotencyKey, useSaveTask } from '../writes/task-writes-api';

/**
 * «Новая задача…» on top of the list (FR-003): Enter or «Добавить» adds a task for `assignee`, Обычно, without a date;
 * the field clears and keeps focus. The pencil opens the full form with the typed title.
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
  const save = useSaveTask();
  const idempotency = useIdempotencyKey();
  const failure = save.error
    ? describeWriteError(save.error, 'добавить')
    : undefined;

  const submit = (event: FormEvent) => {
    event.preventDefault();
    const error = titleError(title);
    setCheck(title.trim() === '' ? undefined : error);
    if (error || save.isPending) return;
    const body = quickTask(title, assignee);
    save.mutate(
      { body, idempotencyKey: idempotency.keyFor(body) },
      {
        onSuccess: () => {
          idempotency.renew();
          setTitle('');
          input.current?.focus();
        },
        onError: (e) => {
          if (describeWriteError(e).renewKey) idempotency.renew();
        },
      },
    );
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
          placeholder="Новая задача…"
          value={title}
          onChange={(event) => {
            setTitle(event.target.value);
            setCheck(undefined);
            if (save.isError) save.reset();
          }}
          error={Boolean(check)}
          helperText={check}
          slotProps={{
            htmlInput: { 'aria-label': 'Новая задача', enterKeyHint: 'done' },
          }}
        />
        <Tooltip title="Добавить">
          <span>
            <IconButton
              type="submit"
              color="primary"
              aria-label="Добавить"
              disabled={save.isPending}
            >
              <AddOutlinedIcon />
            </IconButton>
          </span>
        </Tooltip>
        <Tooltip title="Подробная форма">
          <IconButton
            aria-label="Подробная форма"
            onClick={() => onOpenForm(title.trim())}
          >
            <EditNoteOutlinedIcon />
          </IconButton>
        </Tooltip>
      </Box>
      {failure && <Alert severity="error">{failure.message}</Alert>}
    </Box>
  );
}
