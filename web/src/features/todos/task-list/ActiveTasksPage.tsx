import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import { useState } from 'react';
import { kyivToday } from '../data/due-date';
import { useTasks, type TodoTaskItem } from '../data/todos-api';
import { ListStatus } from '../feedback/ListStatus';
import { LoadMoreButton } from '../feedback/LoadMoreButton';
import { ResultSnackbar } from '../feedback/ResultSnackbar';
import { TaskFilters } from '../task-filters/TaskFilters';
import { TaskDialog } from '../task-dialog/TaskDialog';
import {
  isFiltered,
  quickAddAssignee,
  toTaskQuery,
} from '../url-state/filters';
import { useTodoFilters } from '../url-state/use-todo-filters';
import { QuickAdd } from './QuickAdd';
import { TaskGroups } from './TaskGroups';
import { useDoneTick } from './use-done-tick';

/** What the dialog shows: a task, a new one (with the quick-add text), or nothing. */
type Open = { item: TodoTaskItem } | { title: string } | null;

/**
 * «Задачи»: the filters, quick add, and the active tasks by due day (FR-001…FR-004). A tick marks a task done
 * at once with «Отменить»; a tap opens it.
 */
export function ActiveTasksPage() {
  const filterState = useTodoFilters('active');
  const { filters } = filterState;
  const today = kyivToday();
  const query = useTasks('active', toTaskQuery(filters, 'active', today));
  const done = useDoneTick();
  const [open, setOpen] = useState<Open>(null);
  const assignee = quickAddAssignee(filters.who);

  const pages = query.data?.pages ?? [];
  const items = pages.flatMap((page) => page.items);
  const first = pages[0];

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <TaskFilters view="active" state={filterState} />
      <Card>
        <QuickAdd
          assignee={assignee}
          onOpenForm={(title) => setOpen({ title })}
        />
        <ListStatus
          isPending={query.isPending}
          error={query.error}
          isEmpty={items.length === 0}
          isFiltered={isFiltered(filters)}
          empty={{
            title: 'Задач нет',
            hint: 'Добавьте первую выше.',
          }}
          loadingLabel="Загружаю задачи"
          onRetry={() => void query.refetch()}
          onResetFilters={filterState.clear}
        >
          <TaskGroups
            items={items}
            today={today}
            onDone={done.complete}
            onOpen={(item) => setOpen({ item })}
          />
          {query.hasNextPage && first && (
            <LoadMoreButton
              shown={items.length}
              total={first.totalCount}
              isLoading={query.isFetchingNextPage}
              onClick={() => void query.fetchNextPage()}
            />
          )}
        </ListStatus>
      </Card>
      {open && (
        <TaskDialog
          item={'item' in open ? open.item : undefined}
          initialTitle={'title' in open ? open.title : undefined}
          assignee={assignee}
          onClose={() => setOpen(null)}
        />
      )}
      <ResultSnackbar message={done.message} onClose={done.dismiss} />
    </Box>
  );
}
