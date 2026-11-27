import DeleteOutlineOutlinedIcon from '@mui/icons-material/DeleteOutlineOutlined';
import Checkbox from '@mui/material/Checkbox';
import IconButton from '@mui/material/IconButton';
import InputBase from '@mui/material/InputBase';
import ListItem from '@mui/material/ListItem';
import Tooltip from '@mui/material/Tooltip';
import { useState } from 'react';
import type { TaskStepItem } from '../data/todos-api';
import { titleError } from '../task-dialog/task-form';

interface StepItemProps {
  step: TaskStepItem;
  /** A new task's steps are not saved yet, so they cannot be ticked. */
  canTick: boolean;
  onToggle: () => void;
  onRename: (title: string) => void;
  onRemove: () => void;
}

/** One step: the tick, the title renamed in place (saved on leaving the field or Enter), delete. */
export function StepItem({
  step,
  canTick,
  onToggle,
  onRename,
  onRemove,
}: StepItemProps) {
  const [title, setTitle] = useState(step.title);
  const [synced, setSynced] = useState(step.title);
  // The step was renamed elsewhere or the save came back: show the saved title.
  if (step.title !== synced) {
    setSynced(step.title);
    setTitle(step.title);
  }

  const commit = () => {
    const trimmed = title.trim();
    if (trimmed === step.title) return;
    if (titleError(trimmed)) setTitle(step.title);
    else onRename(trimmed);
  };

  return (
    <ListItem disablePadding sx={{ gap: 0.5, alignItems: 'flex-start' }}>
      <Checkbox
        checked={step.isDone}
        disabled={!canTick}
        onChange={onToggle}
        slotProps={{ input: { 'aria-label': `Крок виконано: ${step.title}` } }}
      />
      <InputBase
        value={title}
        multiline
        fullWidth
        onChange={(event) => setTitle(event.target.value)}
        onBlur={commit}
        onKeyDown={(event) => {
          if (event.key === 'Enter') {
            event.preventDefault();
            (event.target as HTMLElement).blur();
          }
        }}
        inputProps={{ 'aria-label': `Крок: ${step.title}`, maxLength: 200 }}
        sx={{
          py: 1,
          textDecoration: step.isDone ? 'line-through' : undefined,
          color: step.isDone ? 'text.secondary' : 'text.primary',
        }}
      />
      <Tooltip title="Видалити крок">
        <IconButton
          aria-label={`Видалити крок: ${step.title}`}
          onClick={onRemove}
          sx={{ mt: 0.5 }}
        >
          <DeleteOutlineOutlinedIcon fontSize="small" />
        </IconButton>
      </Tooltip>
    </ListItem>
  );
}
