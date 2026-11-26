import Box from '@mui/material/Box';
import List from '@mui/material/List';
import Typography from '@mui/material/Typography';
import { useId } from 'react';
import { dueBucketLabels, groupByDue, type DueBucket } from '../data/due-date';
import type { TodoTaskItem } from '../data/todos-api';
import { TaskRow } from '../task-row/TaskRow';

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

function TaskGroup({
  bucket,
  count,
  children,
}: {
  bucket: DueBucket;
  count: number;
  children: React.ReactNode;
}) {
  const headingId = useId();
  return (
    <Box
      component="section"
      aria-labelledby={headingId}
      sx={{ borderTop: 1, borderColor: 'divider' }}
    >
      <Typography
        id={headingId}
        variant="subtitle2"
        component="h2"
        sx={{
          px: 2,
          pt: 1.5,
          pb: 0.5,
          fontWeight: 600,
          color: bucket === 'overdue' ? 'error.main' : 'text.secondary',
        }}
      >
        {dueBucketLabels[bucket]}{' '}
        <Box component="span" sx={{ fontVariantNumeric: 'tabular-nums' }}>
          · {count}
        </Box>
      </Typography>
      {children}
    </Box>
  );
}
