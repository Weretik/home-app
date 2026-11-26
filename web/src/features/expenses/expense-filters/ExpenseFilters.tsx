import Box from '@mui/material/Box';
import type { Payer } from '../data/expenses-api';
import { useExpenseCategories } from '../data/expenses-api';
import { payerLabels } from '../data/labels';
import { FilterSelect } from '../month-view/FilterSelect';

const payerOptions = (Object.keys(payerLabels) as Payer[]).map((value) => ({
  value,
  label: payerLabels[value],
}));

interface ExpenseFiltersProps {
  categoryId: string | undefined;
  payer: Payer | undefined;
  onCategoryChange: (value: string | undefined) => void;
  onPayerChange: (value: Payer | undefined) => void;
}

/** Category and «Кто» filters of the expense list. */
export function ExpenseFilters({
  categoryId,
  payer,
  onCategoryChange,
  onPayerChange,
}: ExpenseFiltersProps) {
  const categories = useExpenseCategories();
  const categoryOptions = (categories.data ?? []).map((category) => ({
    value: category.id,
    label: category.name,
  }));
  // Keep a category from the URL selectable while the list loads, so the select has a matching option.
  const knownCategory =
    !categoryId || categoryOptions.some((o) => o.value === categoryId);

  return (
    <Box
      role="group"
      aria-label="Фильтры"
      sx={{
        display: 'grid',
        gap: 2,
        gridTemplateColumns: { xs: '1fr 1fr', sm: 'auto auto' },
        justifyContent: 'start',
      }}
    >
      <FilterSelect
        label="Категория"
        value={knownCategory ? categoryId : undefined}
        options={categoryOptions}
        onChange={onCategoryChange}
        disabled={categories.isPending}
      />
      <FilterSelect
        label="Кто"
        value={payer}
        options={payerOptions}
        onChange={onPayerChange}
      />
    </Box>
  );
}
