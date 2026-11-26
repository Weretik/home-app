import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import type { Theme } from '@mui/material/styles';
import useMediaQuery from '@mui/material/useMediaQuery';
import {
  useExpenses,
  type ExpenseItem,
  type Payer,
} from '../data/expenses-api';
import { payerLabels } from '../data/labels';
import { ListStatus } from '../month-view/ListStatus';
import { LoadMoreButton } from '../month-view/LoadMoreButton';
import { useEntryEditor } from '../entry-editing/use-entry-editor';
import { ExpenseEditor } from '../expense-editing/ExpenseEditor';
import { useClearFilters, useFilterParam } from '../url-state/use-filter-param';
import { useMonthParam } from '../url-state/use-month-param';
import { ExpenseFilters } from './ExpenseFilters';
import { ExpenseList } from './ExpenseList';
import { ExpenseTable } from './ExpenseTable';

const payers = Object.keys(payerLabels) as Payer[];

/** «Расходы»: filters, «Добавить», and the month's expenses, newest first, each with edit and delete. */
export function ExpensesPage() {
  const [month] = useMonthParam();
  const [categoryId, setCategoryId] = useFilterParam<string>('category');
  const [payer, setPayer] = useFilterParam<Payer>('payer', payers);
  const clearFilters = useClearFilters();
  const query = useExpenses(month, { categoryId, payer });
  const editor = useEntryEditor<ExpenseItem>();
  const isComputer = useMediaQuery((theme: Theme) =>
    theme.breakpoints.up('md'),
  );

  const pages = query.data?.pages ?? [];
  const items = pages.flatMap((page) => page.items);
  const first = pages[0];

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <Box
        sx={{
          display: 'flex',
          flexWrap: 'wrap',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 2,
        }}
      >
        <ExpenseFilters
          categoryId={categoryId}
          payer={payer}
          onCategoryChange={setCategoryId}
          onPayerChange={setPayer}
        />
        <Button
          variant="contained"
          startIcon={<AddOutlinedIcon />}
          onClick={editor.add}
        >
          Добавить расход
        </Button>
      </Box>
      <Card>
        <ListStatus
          isPending={query.isPending}
          error={query.error}
          isEmpty={items.length === 0}
          isFiltered={Boolean(categoryId || payer)}
          loadingLabel="Загружаю расходы"
          onRetry={() => void query.refetch()}
          onResetFilters={clearFilters}
        >
          {isComputer ? (
            <ExpenseTable
              items={items}
              onEdit={editor.edit}
              onDelete={editor.remove}
            />
          ) : (
            <ExpenseList
              items={items}
              onEdit={editor.edit}
              onDelete={editor.remove}
            />
          )}
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
      <ExpenseEditor editor={editor} month={month} />
    </Box>
  );
}
