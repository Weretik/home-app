import Box from '@mui/material/Box';
import { GiftEditor } from '../gift-editing/GiftEditor';
import { GiftFilters } from '../list-view/GiftFilters';
import { GiftRows } from '../list-view/GiftRows';
import { ListStatus } from '../list-view/ListStatus';
import { StatusSnackbar } from '../list-view/StatusSnackbar';
import { useGiftList } from '../list-view/use-gift-list';

/** «Подаровані»: gifted and dropped gifts per person, the latest gifted first; «Повернути до списку» moves one back. */
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
        loadingLabel="Завантажую подаровані"
        empty={{
          title: 'Тут з’являться подаровані подарунки',
          hint: 'Позначте подарунок «Подаровано» у списку, і він переїде сюди.',
        }}
        onRetry={() => void list.gifts.refetch()}
        onReset={filterState.clear}
      >
        <GiftRows
          items={list.items}
          variant="archive"
          label="Подаровані подарунки"
          grouped={!filterState.filters.recipient}
          isChanging={list.isChanging}
          onStatus={list.changeStatus}
          onEdit={list.editor.edit}
          onDelete={list.editor.remove}
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
