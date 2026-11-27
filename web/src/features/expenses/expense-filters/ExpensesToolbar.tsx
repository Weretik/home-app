import Box from '@mui/material/Box';
import type { ComponentProps } from 'react';
import { PageToolbar } from '../month-view/PageToolbar';
import { ExpenseFilters } from './ExpenseFilters';
import { PeriodFilter } from './PeriodFilter';

interface ExpensesToolbarProps {
  filters: ComponentProps<typeof ExpenseFilters>;
  period: ComponentProps<typeof PeriodFilter>;
  onAdd: () => void;
}

/** The row above the expenses: the category and «Хто» filters, «Період», and «Додати витрату». */
export function ExpensesToolbar({
  filters,
  period,
  onAdd,
}: ExpensesToolbarProps) {
  return (
    <PageToolbar addLabel="Додати витрату" onAdd={onAdd}>
      <Box
        sx={{
          display: 'flex',
          flexWrap: 'wrap',
          alignItems: 'flex-start',
          gap: 2,
        }}
      >
        <ExpenseFilters {...filters} />
        <PeriodFilter {...period} />
      </Box>
    </PageToolbar>
  );
}
