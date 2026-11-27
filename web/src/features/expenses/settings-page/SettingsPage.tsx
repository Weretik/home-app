import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';
import { useBudget, type SpendingLimitItem } from '../budget-data/budget-api';
import { useEntryEditor } from '../entry-editing/use-entry-editor';
import { LimitEditor } from '../limit-editing/LimitEditor';
import { ListStatus } from '../month-view/ListStatus';
import { BudgetSettingsCard } from './BudgetSettingsCard';
import { LimitsCard } from './LimitsCard';

/** «Налаштування» of the Expenses section: the monthly limit, the savings goal and the spending limits. */
export function SettingsPage() {
  const budget = useBudget();
  const editor = useEntryEditor<SpendingLimitItem>();

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <Typography variant="h5" component="h2">
        Налаштування
      </Typography>
      {budget.data ? (
        <>
          <BudgetSettingsCard budget={budget.data} />
          <LimitsCard
            limits={budget.data.limits}
            onAdd={editor.add}
            onEdit={editor.edit}
            onDelete={editor.remove}
          />
        </>
      ) : (
        <Card>
          <ListStatus
            isPending={budget.isPending}
            error={budget.error}
            isEmpty={false}
            loadingLabel="Завантажую налаштування"
            onRetry={() => void budget.refetch()}
          >
            {null}
          </ListStatus>
        </Card>
      )}
      <LimitEditor editor={editor} />
    </Box>
  );
}
