import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Snackbar from '@mui/material/Snackbar';
import Typography from '@mui/material/Typography';
import { useState } from 'react';
import type { ExpenseItem } from '../data/expenses-api';
import { useEntryEditor } from '../entry-editing/use-entry-editor';
import { ExpenseEditor } from '../expense-editing/ExpenseEditor';
import { ExpensesToolbar } from '../expense-filters/ExpensesToolbar';
import { PagedListCard } from '../month-view/PagedListCard';
import { ExpenseList } from './ExpenseList';
import { ExpenseTable } from './ExpenseTable';
import { PendingExpenses } from './PendingExpenses';
import { useShownExpenses } from './use-shown-expenses';

/**
 * «Витрати»: filters, «Період», «Додати», and the expenses of the month or of a period of days, newest first, each
 * with edit and delete. A wrong period shows its reason and loads nothing. An expense added offline waits for the
 * network: the page says so and lists it under «Очікують мережі» until it is sent.
 */
export function ExpensesPage() {
  const shown = useShownExpenses();
  const editor = useEntryEditor<ExpenseItem>();
  const [queued, setQueued] = useState(false);
  const rows = {
    label: shown.listLabel,
    onEdit: editor.edit,
    onDelete: editor.remove,
  };

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <ExpensesToolbar
        filters={shown.filters}
        period={{
          param: shown.periodParam,
          month: shown.month,
          error: shown.periodProblem,
        }}
        onAdd={editor.add}
      />
      <PendingExpenses />
      {shown.periodProblem ? (
        <Card>
          <Typography color="text.secondary" sx={{ p: { xs: 2, md: 3 } }}>
            Виправте дати періоду, щоб побачити витрати.
          </Typography>
        </Card>
      ) : (
        <PagedListCard
          query={shown.query}
          isFiltered={shown.isFiltered}
          scope={shown.periodParam.active ? 'period' : 'month'}
          loadingLabel="Завантажую витрати"
          onResetFilters={shown.clearFilters}
        >
          {(items, isComputer) =>
            isComputer ? (
              <ExpenseTable items={items} {...rows} />
            ) : (
              <ExpenseList items={items} {...rows} />
            )
          }
        </PagedListCard>
      )}
      <ExpenseEditor
        editor={editor}
        newDate={shown.newDate}
        onQueued={() => setQueued(true)}
      />
      <Snackbar
        open={queued}
        autoHideDuration={4000}
        onClose={() => setQueued(false)}
        message="Витрата додасться, коли з’явиться мережа"
      />
    </Box>
  );
}
