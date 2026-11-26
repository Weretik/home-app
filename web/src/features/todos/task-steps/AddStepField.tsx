import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import TextField from '@mui/material/TextField';
import Tooltip from '@mui/material/Tooltip';
import { useState } from 'react';
import { titleError } from '../task-dialog/task-form';

/**
 * «Добавить шаг»: Enter or the plus adds it, the field clears and keeps focus. It is not a form of its own (the dialog
 * is one), so Enter is caught here instead of saving the task.
 */
export function AddStepField({
  disabled,
  onAdd,
}: {
  disabled: boolean;
  onAdd: (title: string, onDone: () => void) => void;
}) {
  const [title, setTitle] = useState('');
  const [error, setError] = useState<string>();

  const add = () => {
    if (disabled || title.trim() === '') return;
    const problem = titleError(title);
    setError(problem);
    if (!problem) onAdd(title.trim(), () => setTitle(''));
  };

  return (
    <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1 }}>
      <TextField
        size="small"
        fullWidth
        placeholder="Добавить шаг"
        value={title}
        error={Boolean(error)}
        helperText={error}
        onChange={(event) => {
          setTitle(event.target.value);
          setError(undefined);
        }}
        onKeyDown={(event) => {
          if (event.key === 'Enter') {
            event.preventDefault();
            add();
          }
        }}
        slotProps={{
          htmlInput: { 'aria-label': 'Новый шаг', enterKeyHint: 'done' },
        }}
      />
      <Tooltip title="Добавить шаг">
        <span>
          <IconButton
            aria-label="Добавить шаг"
            onClick={add}
            disabled={disabled}
          >
            <AddOutlinedIcon />
          </IconButton>
        </span>
      </Tooltip>
    </Box>
  );
}
