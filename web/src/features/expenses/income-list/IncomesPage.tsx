import SavingsOutlinedIcon from '@mui/icons-material/SavingsOutlined';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import type { Theme } from '@mui/material/styles';
import useMediaQuery from '@mui/material/useMediaQuery';
import { useIncomes, type Person } from '../data/expenses-api';
import { personLabels } from '../data/labels';
import { FilterSelect } from '../month-view/FilterSelect';
import { ListStatus } from '../month-view/ListStatus';
import { LoadMoreButton } from '../month-view/LoadMoreButton';
import { MonthTotalCard } from '../month-view/MonthTotalCard';
import { useClearFilters, useFilterParam } from '../url-state/use-filter-param';
import { useMonthParam } from '../url-state/use-month-param';
import { IncomeList } from './IncomeList';
import { IncomeTable } from './IncomeTable';

const people = Object.keys(personLabels) as Person[];
const personOptions = people.map((value) => ({
  value,
  label: personLabels[value],
}));

/** «Доходы»: the month's total, the person filter, and the incomes, newest first. */
export function IncomesPage() {
  const [month] = useMonthParam();
  const [person, setPerson] = useFilterParam<Person>('person', people);
  const clearFilters = useClearFilters();
  const query = useIncomes(month, { person });
  const isComputer = useMediaQuery((theme: Theme) =>
    theme.breakpoints.up('md'),
  );

  const pages = query.data?.pages ?? [];
  const items = pages.flatMap((page) => page.items);
  const first = pages[0];

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <MonthTotalCard
        title="Доходы за месяц"
        icon={SavingsOutlinedIcon}
        color="secondary"
        total={first && { amount: first.totalAmount, count: first.totalCount }}
      />
      <Box role="group" aria-label="Фильтры" sx={{ display: 'flex' }}>
        <FilterSelect
          label="Кто"
          value={person}
          options={personOptions}
          onChange={setPerson}
        />
      </Box>
      <Card>
        <ListStatus
          isPending={query.isPending}
          error={query.error}
          isEmpty={items.length === 0}
          isFiltered={Boolean(person)}
          loadingLabel="Загружаю доходы"
          onRetry={() => void query.refetch()}
          onResetFilters={clearFilters}
        >
          {isComputer ? (
            <IncomeTable items={items} />
          ) : (
            <IncomeList items={items} />
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
