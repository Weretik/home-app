import ReplayOutlinedIcon from '@mui/icons-material/ReplayOutlined';
import Button from '@mui/material/Button';
import { formatDue, kyivDayOf } from '../data/due-date';
import { statusLabels } from '../data/labels';
import type { TodoTaskItem } from '../data/todos-api';
import { TaskRow } from '../task-row/TaskRow';

interface ArchiveRowProps {
  task: TodoTaskItem;
  today: string;
  divider: boolean;
  onOpen: (task: TodoTaskItem) => void;
  onReturn: (task: TodoTaskItem) => void;
}

/** A done or cancelled task: «Сделано сегодня» / «Не актуально пн 23.11» first, then «Вернуть» (FR-006). */
export function ArchiveRow({
  task,
  today,
  divider,
  onOpen,
  onReturn,
}: ArchiveRowProps) {
  const completed = task.completedAtUtc
    ? `${statusLabels[task.status]} ${formatDue(kyivDayOf(task.completedAtUtc), today)}`
    : statusLabels[task.status];

  return (
    <TaskRow
      task={task}
      today={today}
      divider={divider}
      onOpen={onOpen}
      extra={completed}
      action={
        <Button
          size="small"
          startIcon={<ReplayOutlinedIcon fontSize="small" />}
          onClick={() => onReturn(task)}
          aria-label={`Вернуть: ${task.title}`}
        >
          Вернуть
        </Button>
      }
    />
  );
}
