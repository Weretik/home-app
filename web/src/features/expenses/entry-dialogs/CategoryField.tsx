import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';
import { useExpenseCategories } from '../data/expenses-api';
import { fieldMessage, type FieldErrors } from '../entry-editing/entry-fields';

/** «Категория»: one expense category, required; disabled while the categories load. */
export function CategoryField({
  value,
  onChange,
  errors,
}: {
  value: string;
  onChange: (value: string) => void;
  errors: FieldErrors;
}) {
  const categories = useExpenseCategories();
  return (
    <TextField
      select
      label="Категория"
      required
      value={value}
      disabled={categories.isPending}
      onChange={(event) => onChange(event.target.value)}
      {...fieldMessage(errors, 'categoryId')}
    >
      {(categories.data ?? []).map((category) => (
        <MenuItem key={category.id} value={category.id}>
          {category.name}
        </MenuItem>
      ))}
    </TextField>
  );
}
