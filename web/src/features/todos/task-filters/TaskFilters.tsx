import Box from '@mui/material/Box';
import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';
import { categoryLabel, useCategories } from '../data/categories-api';
import type { TaskView } from '../data/todos-api';
import type { useTodoFilters } from '../url-state/use-todo-filters';
import { DateFilter } from './DateFilter';
import { SearchField } from './SearchField';
import { WhoChips } from './WhoChips';

/** «Кому», category, the date filter and search of a task list, kept in the URL (FR-002). They wrap on a phone. */
export function TaskFilters({
  view,
  state,
}: {
  view: TaskView;
  state: ReturnType<typeof useTodoFilters>;
}) {
  const categories = useCategories();
  const { filters } = state;
  const options = categories.data ?? [];
  // Keep a category from the URL selectable while the list loads, so the select has a matching option.
  const known =
    !filters.category ||
    filters.category === 'none' ||
    options.some((c) => c.id === filters.category);

  return (
    <Box
      role="group"
      aria-label="Фільтри"
      sx={{ display: 'flex', flexWrap: 'wrap', alignItems: 'center', gap: 1.5 }}
    >
      <WhoChips value={filters.who} onChange={state.setWho} />
      <TextField
        select
        size="small"
        label="Категорія"
        value={known ? (filters.category ?? '') : ''}
        disabled={categories.isPending}
        onChange={(event) => state.setCategory(event.target.value || undefined)}
        sx={{ minWidth: 0, flex: { xs: '1 1 100%', sm: '0 1 200px' } }}
        slotProps={{
          select: { displayEmpty: true },
          inputLabel: { shrink: true },
        }}
      >
        <MenuItem value="">Усі</MenuItem>
        <MenuItem value="none">Без категорії</MenuItem>
        {options.map((category) => (
          <MenuItem key={category.id} value={category.id}>
            {categoryLabel(category)}
          </MenuItem>
        ))}
      </TextField>
      <DateFilter view={view} state={state} />
      <SearchField value={filters.q} onChange={state.setSearch} />
    </Box>
  );
}
