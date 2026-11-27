import ReplayOutlinedIcon from '@mui/icons-material/ReplayOutlined';
import Button from '@mui/material/Button';
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

/** A done or cancelled task under its completion day: «Зроблено» / «Неактуально», then «Повернути» (FR-006). */
export function ArchiveRow({
  task,
  today,
  divider,
  onOpen,
  onReturn,
}: ArchiveRowProps) {
  return (
    <TaskRow
      task={task}
      today={today}
      divider={divider}
      onOpen={onOpen}
      extra={statusLabels[task.status]}
      action={
        <Button
          size="small"
          startIcon={<ReplayOutlinedIcon fontSize="small" />}
          onClick={() => onReturn(task)}
          aria-label={`Повернути: ${task.title}`}
        >
          Повернути
        </Button>
      }
    />
  );
}
