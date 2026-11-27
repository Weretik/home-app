import Box from '@mui/material/Box';
import { useIncomes, type IncomeItem, type Person } from '../data/expenses-api';
import { personLabels } from '../data/labels';
import { useEntryEditor } from '../entry-editing/use-entry-editor';
import { IncomeEditor } from '../income-editing/IncomeEditor';
import { FilterSelect } from '../month-view/FilterSelect';
import { PagedListCard } from '../month-view/PagedListCard';
import { PageToolbar } from '../month-view/PageToolbar';
import { useClearFilters, useFilterParam } from '../url-state/use-filter-param';
import { useMonthParam } from '../url-state/use-month-param';
import { IncomeList } from './IncomeList';
import { IncomeTable } from './IncomeTable';

const people = Object.keys(personLabels) as Person[];
const personOptions = people.map((value) => ({
  value,
  label: personLabels[value],
}));

/** «Доходи»: the person filter, «Додати», and the month's incomes, newest first, each with edit and delete. */
export function IncomesPage() {
  const [month] = useMonthParam();
  const [person, setPerson] = useFilterParam<Person>('person', people);
  const clearFilters = useClearFilters();
  const query = useIncomes(month, { person });
  const editor = useEntryEditor<IncomeItem>();

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <PageToolbar addLabel="Додати дохід" onAdd={editor.add}>
        <Box role="group" aria-label="Фільтри" sx={{ display: 'flex' }}>
          <FilterSelect
            label="Хто"
            value={person}
            options={personOptions}
            onChange={setPerson}
          />
        </Box>
      </PageToolbar>
      <PagedListCard
        query={query}
        isFiltered={Boolean(person)}
        loadingLabel="Завантажую доходи"
        onResetFilters={clearFilters}
      >
        {(items, isComputer) =>
          isComputer ? (
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
          )
        }
      </PagedListCard>
      <IncomeEditor editor={editor} month={month} />
    </Box>
  );
}
