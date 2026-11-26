import List from '@mui/material/List';
import { dueBucketLabels, groupByDue } from '../data/due-date';
import type { TodoTaskItem } from '../data/todos-api';
import { TaskRow } from '../task-row/TaskRow';
import { TaskGroup } from './TaskGroup';

interface TaskGroupsProps {
  items: TodoTaskItem[];
  today: string;
  onDone: (task: TodoTaskItem) => void;
  onOpen: (task: TodoTaskItem) => void;
}

/** The active tasks in their groups (FR-001): Просрочено, Сегодня, Скоро, Позже, Без срока; empty ones hidden. */
export function TaskGroups({ items, today, onDone, onOpen }: TaskGroupsProps) {
  return groupByDue(items, today).map((group) => (
    <TaskGroup
      key={group.bucket}
      bucket={group.bucket}
      count={group.items.length}
    >
      <List disablePadding aria-label={dueBucketLabels[group.bucket]}>
        {group.items.map((task, index) => (
          <TaskRow
            key={task.id}
            task={task}
            today={today}
            divider={index < group.items.length - 1}
            onDone={onDone}
            onOpen={onOpen}
          />
        ))}
      </List>
    </TaskGroup>
  ));
}
