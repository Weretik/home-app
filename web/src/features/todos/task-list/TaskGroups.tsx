import List from '@mui/material/List';
import { groupByDueDay } from '../data/day-groups';
import type { TodoTaskItem } from '../data/todos-api';
import { TaskRow } from '../task-row/TaskRow';
import { TaskGroup } from './TaskGroup';

interface TaskGroupsProps {
  items: TodoTaskItem[];
  today: string;
  onDone: (task: TodoTaskItem) => void;
  onOpen: (task: TodoTaskItem) => void;
}

/** The active tasks by due day (FR-001): Прострочено, Сьогодні, Завтра, one group per later day, Без терміну. */
export function TaskGroups({ items, today, onDone, onOpen }: TaskGroupsProps) {
  return groupByDueDay(items, today).map((group) => (
    <TaskGroup
      key={group.key}
      label={group.label}
      count={group.items.length}
      overdue={group.overdue}
    >
      <List disablePadding aria-label={group.label}>
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
