import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Snackbar from '@mui/material/Snackbar';
import { useState } from 'react';
import { useDeleteItem } from '../data/item-writes-api';
import { itemTitle } from '../data/labels';
import type { ShoppingItem } from '../data/shopping-api';
import { describeWriteError } from '../data/write-failure-api';
import { ConfirmDialog } from '../page-parts/ConfirmDialog';
import { PageStatus } from '../page-parts/PageStatus';
import { HistoryMonth } from './HistoryMonth';
import { groupByMonth } from './history-groups';
import { useShoppingHistory } from './history-api';
import { useReturnItem } from './use-return-item';

/** «История»: bought items by month and day (SDD 003), «Вернуть в список», delete, «Показать ещё» (R-10, R-15). */
export function HistoryPage() {
  const history = useShoppingHistory();
  const remove = useDeleteItem();
  const back = useReturnItem();
  const [deleting, setDeleting] = useState<ShoppingItem | null>(null);
  const items = history.data?.pages.flatMap((page) => page.items) ?? [];

  const closeDelete = () => {
    remove.reset();
    setDeleting(null);
  };

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <PageStatus
        isPending={history.isPending}
        error={history.error}
        hasData={history.data !== undefined}
        isEmpty={items.length === 0}
        loadingLabel="Загружаю историю"
        empty={{
          emoji: '🧾',
          title: 'Истории пока нет',
          hint: 'Купленное попадает сюда после «Убрать купленные» или само через несколько часов.',
        }}
        onRetry={() => void history.refetch()}
      >
        {groupByMonth(items, new Date(), history.hasNextPage).map((month) => (
          <HistoryMonth
            key={month.key}
            month={month}
            returning={back.returning}
            onReturn={back.returnItem}
            onDelete={setDeleting}
          />
        ))}
        {history.hasNextPage && (
          <Button
            onClick={() => void history.fetchNextPage()}
            disabled={history.isFetchingNextPage}
            sx={{ justifySelf: 'center' }}
          >
            {history.isFetchingNextPage ? 'Загружаю…' : 'Показать ещё'}
          </Button>
        )}
      </PageStatus>
      {history.isFetchNextPageError && (
        <Alert severity="error">
          Не удалось загрузить ещё. Попробуйте снова.
        </Alert>
      )}
      {deleting && (
        <ConfirmDialog
          title="Удалить из истории?"
          summary={itemTitle(deleting)}
          error={
            remove.error
              ? describeWriteError(remove.error, 'удалить').message
              : undefined
          }
          isPending={remove.isPending}
          onConfirm={() => remove.mutate(deleting, { onSuccess: closeDelete })}
          onClose={closeDelete}
        />
      )}
      <Snackbar
        open={back.notice !== null}
        autoHideDuration={4000}
        onClose={back.clearNotice}
        message={back.notice ?? ''}
      />
    </Box>
  );
}
