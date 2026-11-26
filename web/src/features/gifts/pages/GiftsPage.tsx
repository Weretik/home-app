import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import { GiftEditor } from '../gift-editing/GiftEditor';
import { GiftFilters } from '../list-view/GiftFilters';
import { GiftRows } from '../list-view/GiftRows';
import { ListStatus } from '../list-view/ListStatus';
import { StatusSnackbar } from '../list-view/StatusSnackbar';
import { useGiftList } from '../list-view/use-gift-list';

/** «Список»: the wanted gifts in a block per person unless one is picked; «Подарено» moves a gift to «Подаренные». */
export function GiftsPage() {
  const list = useGiftList(false);
  const { filterState } = list;

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <GiftFilters
        state={filterState}
        categories={list.categories}
        action={
          <Button
            variant="contained"
            startIcon={<AddOutlinedIcon />}
            onClick={list.add}
          >
            Добавить подарок
          </Button>
        }
      />
      <ListStatus
        isPending={list.gifts.isPending}
        error={list.gifts.error}
        isEmpty={list.items.length === 0}
        isFiltered={filterState.isFiltered && list.all.length > 0}
        loadingLabel="Загружаю список подарков"
        empty={{
          title: 'Список желаний пуст',
          hint: 'Добавьте первый подарок: для кого, что и, если знаете, ссылку и цену.',
        }}
        onRetry={() => void list.gifts.refetch()}
        onReset={filterState.clear}
      >
        <GiftRows
          items={list.items}
          variant="active"
          label="Список подарков"
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
