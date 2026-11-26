import ReceiptLongOutlinedIcon from '@mui/icons-material/ReceiptLongOutlined';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import type { Theme } from '@mui/material/styles';
import useMediaQuery from '@mui/material/useMediaQuery';
import { useExpenses, type Payer } from '../data/expenses-api';
import { payerLabels } from '../data/labels';
import { ListStatus } from '../month-view/ListStatus';
import { LoadMoreButton } from '../month-view/LoadMoreButton';
import { MonthTotalCard } from '../month-view/MonthTotalCard';
import { useClearFilters, useFilterParam } from '../url-state/use-filter-param';
import { useMonthParam } from '../url-state/use-month-param';
import { ExpenseFilters } from './ExpenseFilters';
import { ExpenseList } from './ExpenseList';
import { ExpenseTable } from './ExpenseTable';

const payers = Object.keys(payerLabels) as Payer[];

/** «Расходы»: the month's total, filters, and the expenses, newest first. */
export function ExpensesPage() {
  const [month] = useMonthParam();
  const [categoryId, setCategoryId] = useFilterParam<string>('category');
  const [payer, setPayer] = useFilterParam<Payer>('payer', payers);
  const clearFilters = useClearFilters();
  const query = useExpenses(month, { categoryId, payer });
  const isComputer = useMediaQuery((theme: Theme) =>
    theme.breakpoints.up('md'),
  );

  const pages = query.data?.pages ?? [];
  const items = pages.flatMap((page) => page.items);
  const first = pages[0];

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <MonthTotalCard
        title="Расходы за месяц"
        icon={ReceiptLongOutlinedIcon}
        color="primary"
        total={first && { amount: first.totalAmount, count: first.totalCount }}
      />
      <ExpenseFilters
        categoryId={categoryId}
        payer={payer}
        onCategoryChange={setCategoryId}
        onPayerChange={setPayer}
      />
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
            <ExpenseTable items={items} />
          ) : (
            <ExpenseList items={items} />
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
    </Box>
  );
}
