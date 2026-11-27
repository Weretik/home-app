import Box from '@mui/material/Box';
import type { useNoteFilters } from '../url-state/use-note-filters';
import { CategorySelect } from './CategorySelect';
import { KindChips } from './KindChips';
import { SearchField } from './SearchField';

/** The kind chips, the category and the search on one wrapping row, kept in the URL and across the tabs (FR-002). */
export function NoteFilters({
  state,
}: {
  state: ReturnType<typeof useNoteFilters>;
}) {
  const { filters } = state;
  return (
    <Box
      role="group"
      aria-label="Фільтри"
      sx={{ display: 'flex', flexWrap: 'wrap', alignItems: 'center', gap: 1.5 }}
    >
      <KindChips value={filters.kind} onChange={state.setKind} />
      <CategorySelect value={filters.category} onChange={state.setCategory} />
      <SearchField value={filters.q} onChange={state.setSearch} />
    </Box>
  );
}
