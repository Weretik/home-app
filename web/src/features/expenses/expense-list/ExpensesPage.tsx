import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';
import type { Theme } from '@mui/material/styles';
import useMediaQuery from '@mui/material/useMediaQuery';
import {
  useExpenses,
  type ExpenseItem,
  type Payer,
} from '../data/expenses-api';
import { payerLabels } from '../data/labels';
import { monthDays } from '../data/month';
import { ListStatus } from '../month-view/ListStatus';
import { LoadMoreButton } from '../month-view/LoadMoreButton';
import { defaultDateIn } from '../entry-editing/entry-fields';
import { useEntryEditor } from '../entry-editing/use-entry-editor';
import { ExpenseEditor } from '../expense-editing/ExpenseEditor';
import { useClearFilters, useFilterParam } from '../url-state/use-filter-param';
import { useMonthParam } from '../url-state/use-month-param';
import { periodError } from '../url-state/period';
import { usePeriodParam } from '../url-state/use-period-param';
import { ExpenseFilters } from './ExpenseFilters';
import { ExpenseList } from './ExpenseList';
import { ExpenseTable } from './ExpenseTable';
import { PeriodFilter } from './PeriodFilter';

const payers = Object.keys(payerLabels) as Payer[];

/**
 * «Расходы»: filters, «Период», «Добавить», and the expenses of the month or of a period of days, newest first, each
 * with edit and delete. A wrong period shows its reason and loads nothing.
 */
export function ExpensesPage() {
  const [month] = useMonthParam();
  const periodParam = usePeriodParam();
  const [categoryId, setCategoryId] = useFilterParam<string>('category');
  const [payer, setPayer] = useFilterParam<Payer>('payer', payers);
  const clearFilters = useClearFilters();
  const error = periodParam.active
    ? periodError(periodParam.period)
    : undefined;
  const period = periodParam.active && !error ? periodParam.period : undefined;
  const query = useExpenses(period ?? month, { categoryId, payer }, !error);
  const editor = useEntryEditor<ExpenseItem>();
  const isComputer = useMediaQuery((theme: Theme) =>
    theme.breakpoints.up('md'),
  );

  const pages = query.data?.pages ?? [];
  const items = pages.flatMap((page) => page.items);
  const first = pages[0];
  const listLabel = periodParam.active
    ? 'Расходы за период'
    : 'Расходы за месяц';
  const newDate = defaultDateIn(period ?? monthDays(month));

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
        <Box
          sx={{
            display: 'flex',
            flexWrap: 'wrap',
            alignItems: 'flex-start',
            gap: 2,
          }}
        >
          <ExpenseFilters
            categoryId={categoryId}
            payer={payer}
            onCategoryChange={setCategoryId}
            onPayerChange={setPayer}
          />
          <PeriodFilter param={periodParam} month={month} error={error} />
        </Box>
        <Button
          variant="contained"
          startIcon={<AddOutlinedIcon />}
          onClick={editor.add}
        >
          Добавить расход
        </Button>
      </Box>
      <Card>
        {error ? (
          <Typography color="text.secondary" sx={{ p: { xs: 2, md: 3 } }}>
            Исправьте даты периода, чтобы увидеть расходы.
          </Typography>
        ) : (
          <ListStatus
            isPending={query.isPending}
            error={query.error}
            isEmpty={items.length === 0}
            isFiltered={Boolean(categoryId || payer)}
            scope={periodParam.active ? 'period' : 'month'}
            loadingLabel="Загружаю расходы"
            onRetry={() => void query.refetch()}
            onResetFilters={clearFilters}
          >
            {isComputer ? (
              <ExpenseTable
                items={items}
                label={listLabel}
                onEdit={editor.edit}
                onDelete={editor.remove}
              />
            ) : (
              <ExpenseList
                items={items}
                label={listLabel}
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
        )}
      </Card>
      <ExpenseEditor editor={editor} newDate={newDate} />
    </Box>
  );
}
