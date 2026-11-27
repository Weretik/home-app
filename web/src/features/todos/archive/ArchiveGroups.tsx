import List from '@mui/material/List';
import { groupByCompletionDay } from '../data/day-groups';
import type { TodoTaskItem } from '../data/todos-api';
import { TaskGroup } from '../task-list/TaskGroup';
import { ArchiveRow } from './ArchiveRow';

interface ArchiveGroupsProps {
  items: TodoTaskItem[];
  today: string;
  onOpen: (task: TodoTaskItem) => void;
  onReturn: (task: TodoTaskItem) => void;
}

/** The archive by the Kyiv day of completion (FR-006): Сьогодні, Вчора, then «Пт, 20 листопада» and older. */
export function ArchiveGroups({
  items,
  today,
  onOpen,
  onReturn,
}: ArchiveGroupsProps) {
  return groupByCompletionDay(items, today).map((group) => (
    <TaskGroup key={group.key} label={group.label} count={group.items.length}>
      <List disablePadding aria-label={group.label}>
        {group.items.map((task, index) => (
          <ArchiveRow
            key={task.id}
            task={task}
            today={today}
            divider={index < group.items.length - 1}
            onOpen={onOpen}
            onReturn={onReturn}
          />
        ))}
      </List>
    </TaskGroup>
  ));
}
