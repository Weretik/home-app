import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import { useState } from 'react';
import { kyivToday } from '../data/due-date';
import { useTasks, type TodoTaskItem } from '../data/todos-api';
import { ListStatus } from '../feedback/ListStatus';
import { LoadMoreButton } from '../feedback/LoadMoreButton';
import { ResultSnackbar, type ResultMessage } from '../feedback/ResultSnackbar';
import { TaskFilters } from '../task-filters/TaskFilters';
import { TaskDialog } from '../task-dialog/TaskDialog';
import { isFiltered, toTaskQuery } from '../url-state/filters';
import { useTodoFilters } from '../url-state/use-todo-filters';
import { describeWriteError } from '../writes/failures-api';
import { useMoveTask } from '../writes/status-api';
import { ArchiveGroups } from './ArchiveGroups';

/**
 * «Архив»: done and cancelled tasks by the day they were completed, the latest first, with the same filters (the
 * date filter reads the completion day) and «Показать ещё» (FR-006).
 * «Вернуть» makes a task «Ждёт» again; it leaves the archive at once.
 */
export function ArchivePage() {
  const filterState = useTodoFilters('archive');
  const { filters } = filterState;
  const today = kyivToday();
  const query = useTasks('archive', toTaskQuery(filters, 'archive', today));
  const move = useMoveTask();
  const [message, setMessage] = useState<ResultMessage | null>(null);
  const [open, setOpen] = useState<TodoTaskItem | null>(null);

  const pages = query.data?.pages ?? [];
  const items = pages.flatMap((page) => page.items);
  const first = pages[0];

  const giveBack = (task: TodoTaskItem) => {
    setMessage({
      text: `Вернули в задачи: ${task.title}`,
      severity: 'success',
    });
    void move
      .mutateAsync({ id: task.id, version: task.version, status: 'Todo' })
      .catch((error: Error) =>
        setMessage({
          text: describeWriteError(error, 'вернуть задачу').message,
          severity: 'error',
        }),
      );
  };

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <TaskFilters view="archive" state={filterState} />
      <Card>
        <ListStatus
          isPending={query.isPending}
          error={query.error}
          isEmpty={items.length === 0}
          isFiltered={isFiltered(filters)}
          empty={{
            title: 'Архив пуст',
            hint: 'Сделанные и неактуальные задачи появятся здесь.',
          }}
          loadingLabel="Загружаю архив"
          onRetry={() => void query.refetch()}
          onResetFilters={filterState.clear}
        >
          <ArchiveGroups
            items={items}
            today={today}
            onOpen={setOpen}
            onReturn={giveBack}
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
          item={open}
          assignee={open.assignee}
          onClose={() => setOpen(null)}
        />
      )}
      <ResultSnackbar message={message} onClose={() => setMessage(null)} />
    </Box>
  );
}
