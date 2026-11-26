import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import type { Theme } from '@mui/material/styles';
import useMediaQuery from '@mui/material/useMediaQuery';
import { useIncomes, type IncomeItem, type Person } from '../data/expenses-api';
import { personLabels } from '../data/labels';
import { useEntryEditor } from '../entry-editing/use-entry-editor';
import { IncomeEditor } from '../income-editing/IncomeEditor';
import { FilterSelect } from '../month-view/FilterSelect';
import { ListStatus } from '../month-view/ListStatus';
import { LoadMoreButton } from '../month-view/LoadMoreButton';
import { useClearFilters, useFilterParam } from '../url-state/use-filter-param';
import { useMonthParam } from '../url-state/use-month-param';
import { IncomeList } from './IncomeList';
import { IncomeTable } from './IncomeTable';

const people = Object.keys(personLabels) as Person[];
const personOptions = people.map((value) => ({
  value,
  label: personLabels[value],
}));

/** «Доходы»: the person filter, «Добавить», and the month's incomes, newest first, each with edit and delete. */
export function IncomesPage() {
  const [month] = useMonthParam();
  const [person, setPerson] = useFilterParam<Person>('person', people);
  const clearFilters = useClearFilters();
  const query = useIncomes(month, { person });
  const editor = useEntryEditor<IncomeItem>();
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
        <Box role="group" aria-label="Фильтры" sx={{ display: 'flex' }}>
          <FilterSelect
            label="Кто"
            value={person}
            options={personOptions}
            onChange={setPerson}
          />
        </Box>
        <Button
          variant="contained"
          startIcon={<AddOutlinedIcon />}
          onClick={editor.add}
        >
          Добавить доход
        </Button>
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
            <IncomeTable
              items={items}
              onEdit={editor.edit}
              onDelete={editor.remove}
            />
          ) : (
            <IncomeList
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
      <IncomeEditor editor={editor} month={month} />
    </Box>
  );
}
