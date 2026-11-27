import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';
import { categoryLabel, useCategories } from '../data/categories-api';

/** The category filter: «Усі», «Без категорії» (`none`) or one category (FR-002). */
export function CategorySelect({
  value,
  onChange,
}: {
  value: string | undefined;
  onChange: (value: string | undefined) => void;
}) {
  const categories = useCategories();
  const options = categories.data ?? [];
  // Keep a category from the URL selectable while the list loads, so the select has a matching option.
  const known =
    !value || value === 'none' || options.some((c) => c.id === value);

  return (
    <TextField
      select
      size="small"
      label="Категорія"
      value={known ? (value ?? '') : ''}
      disabled={categories.isPending}
      onChange={(event) => onChange(event.target.value || undefined)}
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
  );
}
