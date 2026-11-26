import Box from '@mui/material/Box';
import { GiftCard } from '../gift-card/GiftCard';
import { GiftEditor } from '../gift-editing/GiftEditor';
import { GiftFilters } from '../list-view/GiftFilters';
import { GiftGroups } from '../list-view/GiftGroups';
import { ListStatus } from '../list-view/ListStatus';
import { StatusSnackbar } from '../list-view/StatusSnackbar';
import { useGiftList } from '../list-view/use-gift-list';

/** «Подаренные»: gifted and dropped gifts, the latest gifted first; «Вернуть в список» moves one back. */
export function ArchivePage() {
  const list = useGiftList(true);
  const { filterState } = list;

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <GiftFilters state={filterState} categories={list.categories} />
      <ListStatus
        isPending={list.gifts.isPending}
        error={list.gifts.error}
        isEmpty={list.items.length === 0}
        isFiltered={filterState.isFiltered && list.all.length > 0}
        loadingLabel="Загружаю подаренные"
        empty={{
          title: 'Здесь появятся подаренные подарки',
          hint: 'Отметьте подарок «Подарено» в списке, и он переедет сюда.',
        }}
        onRetry={() => void list.gifts.refetch()}
        onReset={filterState.clear}
      >
        <GiftGroups
          items={list.items}
          grouped={false}
          renderCard={(gift) => (
            <GiftCard
              key={gift.id}
              gift={gift}
              variant="archive"
              showRecipient={!filterState.filters.recipient}
              isPending={list.isChanging(gift)}
              onStatus={list.changeStatus}
              onEdit={list.editor.edit}
              onDelete={list.editor.remove}
            />
          )}
        />
      </ListStatus>
      <GiftEditor
        state={list.editor.state}
        categories={list.categories}
        onClose={list.editor.close}
      />
      <StatusSnackbar
        message={list.statusError}
        onClose={list.clearStatusError}
      />
    </Box>
  );
}
