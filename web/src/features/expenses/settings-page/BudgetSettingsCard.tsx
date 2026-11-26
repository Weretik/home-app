import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';
import { useState } from 'react';
import type { BudgetView } from '../budget-data/budget-api';
import { useSaveBudgetSettings } from '../budget-data/budget-writes-api';
import { AmountField } from '../entry-dialogs/AmountField';
import { useEntryForm } from '../entry-editing/use-entry-form';
import {
  budgetSettingsValues,
  toBudgetSettingsRequest,
  validateBudgetSettings,
  type BudgetSettingsValues,
} from './budget-settings-form';

/** «Общий лимит расходов в месяц» and «План копилки»: one small form, saved with the budget's version in If-Match. */
export function BudgetSettingsCard({ budget }: { budget: BudgetView }) {
  const [saved, setSaved] = useState(false);
  const form = useEntryForm({
    initial: budgetSettingsValues(budget),
    validate: validateBudgetSettings,
    toBody: toBudgetSettingsRequest,
    save: useSaveBudgetSettings(budget.version),
    onSaved: () => setSaved(true),
  });
  const set = (name: keyof BudgetSettingsValues, value: string) => {
    setSaved(false);
    form.set(name, value);
  };

  return (
    <Card
      component="form"
      noValidate
      aria-labelledby="budget-settings-title"
      onSubmit={(event) => {
        event.preventDefault();
        setSaved(false);
        form.submit();
      }}
      sx={{ p: { xs: 2, md: 3 }, display: 'grid', gap: 2 }}
    >
      <Typography
        id="budget-settings-title"
        variant="subtitle1"
        component="h3"
        sx={{ fontWeight: 600 }}
      >
        Бюджет и копилка
      </Typography>
      {form.message && <Alert severity="error">{form.message}</Alert>}
      <Box
        sx={{
          display: 'grid',
          gap: 2,
          gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' },
          maxWidth: 720,
        }}
      >
        <AmountField
          label="Общий лимит расходов в месяц"
          name="monthlyLimit"
          required={false}
          hint="Пусто — без лимита"
          value={form.values.monthlyLimit}
          onChange={(value) => set('monthlyLimit', value)}
          errors={form.errors}
        />
        <AmountField
          label="План копилки"
          name="savingsGoal"
          required={false}
          hint="Сколько хотим накопить; пусто — без плана"
          value={form.values.savingsGoal}
          onChange={(value) => set('savingsGoal', value)}
          errors={form.errors}
        />
      </Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
        <Button type="submit" variant="contained" disabled={form.isPending}>
          {form.isPending ? 'Сохраняю…' : 'Сохранить'}
        </Button>
        {saved && (
          <Typography role="status" color="text.secondary">
            Сохранено
          </Typography>
        )}
      </Box>
    </Card>
  );
}
