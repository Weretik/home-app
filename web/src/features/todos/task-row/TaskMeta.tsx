import ChecklistOutlinedIcon from '@mui/icons-material/ChecklistOutlined';
import NotesOutlinedIcon from '@mui/icons-material/NotesOutlined';
import Box from '@mui/material/Box';
import Chip from '@mui/material/Chip';
import { categoryLabel } from '../data/categories-api';
import { formatDue, isOverdue } from '../data/due-date';
import {
  activeStatuses,
  priorityLabels,
  priorityMarks,
  statusLabels,
} from '../data/labels';
import type { TodoTaskItem } from '../data/todos-api';
import { AssigneeAvatar } from './AssigneeAvatar';

const item = {
  display: 'inline-flex',
  alignItems: 'center',
  gap: 0.5,
  whiteSpace: 'nowrap',
} as const;

/**
 * The wrapping line under a task's title (FR-004): a 🎁 for a gift's to-do (gifts 002), priority mark, due date (red when overdue), category, steps,
 * a notes icon, the assignee, and «В работе» / «Отложено». `extra` goes first (the archive's status and date, so an
 * archived row shows no status chip again).
 */
export function TaskMeta({
  task,
  today,
  extra,
}: {
  task: TodoTaskItem;
  today: string;
  extra?: string;
}) {
  const mark = priorityMarks[task.priority];
  const overdue = task.completedAtUtc == null && isOverdue(task.dueDate, today);

  return (
    <Box
      component="span"
      sx={{
        display: 'flex',
        flexWrap: 'wrap',
        alignItems: 'center',
        columnGap: 1.25,
        rowGap: 0.5,
        mt: 0.5,
        typography: 'body2',
        color: 'text.secondary',
        fontVariantNumeric: 'tabular-nums',
      }}
    >
      {extra && <Box component="span">{extra}</Box>}
      {task.sourceKind === 'gift' && (
        <Box
          component="span"
          role="img"
          aria-label="Задача из подарков"
          title="Задача из подарков"
        >
          🎁
        </Box>
      )}
      {mark && (
        <Box
          component="span"
          role="img"
          aria-label={priorityLabels[task.priority]}
          title={priorityLabels[task.priority]}
        >
          {mark}
        </Box>
      )}
      {task.dueDate && (
        <Box
          component="span"
          sx={{
            ...item,
            color: overdue ? 'error.main' : undefined,
            fontWeight: overdue ? 600 : undefined,
          }}
        >
          {overdue && (
            <Box
              component="span"
              sx={{
                position: 'absolute',
                width: 1,
                height: 1,
                overflow: 'hidden',
                clip: 'rect(0 0 0 0)',
              }}
            >
              просрочено:
            </Box>
          )}
          {formatDue(task.dueDate, today)}
        </Box>
      )}
      {task.categoryName && (
        <Chip
          size="small"
          variant="outlined"
          label={categoryLabel({
            name: task.categoryName,
            emoji: task.categoryEmoji,
          })}
          sx={{ height: 22, maxWidth: '100%' }}
        />
      )}
      {task.stepsTotal > 0 && (
        <Box
          component="span"
          sx={item}
          aria-label={`Шаги: ${task.stepsDone} из ${task.stepsTotal}`}
        >
          <ChecklistOutlinedIcon sx={{ fontSize: 16 }} aria-hidden />
          {task.stepsDone}/{task.stepsTotal}
        </Box>
      )}
      {task.notes && (
        <NotesOutlinedIcon sx={{ fontSize: 16 }} titleAccess="Есть заметка" />
      )}
      <AssigneeAvatar assignee={task.assignee} />
      {task.status !== 'Todo' && activeStatuses.includes(task.status) && (
        <Chip
          size="small"
          label={statusLabels[task.status]}
          color={task.status === 'InProgress' ? 'primary' : 'default'}
          sx={{ height: 22 }}
        />
      )}
    </Box>
  );
}
