import Box from '@mui/material/Box';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import type { ReactNode } from 'react';
import type { TodoTaskItem } from '../data/todos-api';
import { DoneCheckbox } from './DoneCheckbox';
import { TaskMeta } from './TaskMeta';

interface TaskRowProps {
  task: TodoTaskItem;
  today: string;
  divider: boolean;
  /** The tick is being saved: the title is struck through and the tick is off. */
  saving?: boolean;
  onDone?: (task: TodoTaskItem) => void;
  onOpen: (task: TodoTaskItem) => void;
  /** The first part of the meta line (the archive's completion date). */
  extra?: string;
  /** An action at the end of the row (the archive's «Вернуть»). */
  action?: ReactNode;
}

/** One task: the tick, then the title and its meta line; a tap on the text opens the task (FR-004). */
export function TaskRow({
  task,
  today,
  divider,
  saving = false,
  onDone,
  onOpen,
  extra,
  action,
}: TaskRowProps) {
  return (
    <ListItem
      disablePadding
      divider={divider}
      sx={{ alignItems: 'flex-start', pl: onDone ? 1 : 0, pr: action ? 1 : 0 }}
    >
      {onDone && (
        <Box sx={{ pt: 1.5 }}>
          <DoneCheckbox
            title={task.title}
            checked={saving}
            disabled={saving}
            onCheck={() => onDone(task)}
          />
        </Box>
      )}
      <ListItemButton
        onClick={() => onOpen(task)}
        sx={{ display: 'block', py: 1.25, px: onDone ? 1 : 2, minWidth: 0 }}
      >
        <Box
          component="span"
          sx={{
            display: 'block',
            overflowWrap: 'anywhere',
            fontWeight: 500,
            textDecoration: saving ? 'line-through' : undefined,
            color: saving ? 'text.secondary' : 'text.primary',
          }}
        >
          {task.title}
        </Box>
        <TaskMeta task={task} today={today} extra={extra} />
      </ListItemButton>
      {action && (
        <Box sx={{ alignSelf: 'center', flexShrink: 0 }}>{action}</Box>
      )}
    </ListItem>
  );
}
