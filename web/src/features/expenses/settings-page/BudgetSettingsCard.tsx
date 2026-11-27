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
import { MonthField } from '../limit-editing/MonthField';
import { monthlyLimitHint } from './amount-steps';
import {
  budgetSettingsValues,
  toBudgetSettingsRequest,
  validateBudgetSettings,
  type BudgetSettingsValues,
} from './budget-settings-form';

/**
 * «Загальний ліміт витрат на місяць» with the month it applies from («Ліміт діє з»), the amount before and the one
 * scheduled, and «План скарбнички»: one small form, saved with the budget's version in If-Match. The month is sent only
 * when the limit or the month changed (R-WLA-003).
 */
export function BudgetSettingsCard({ budget }: { budget: BudgetView }) {
  const [saved, setSaved] = useState(false);
  const [initial] = useState(() => budgetSettingsValues(budget));
  const form = useEntryForm({
    initial,
    validate: validateBudgetSettings,
    toBody: (values) =>
      toBudgetSettingsRequest(values, {
        monthlyLimit: budget.monthlyLimit,
        monthlyLimitFrom: initial.monthlyLimitFrom,
      }),
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
        Бюджет і скарбничка
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
          label="Загальний ліміт витрат на місяць"
          name="monthlyLimit"
          required={false}
          hint={monthlyLimitHint(budget.monthlyLimits)}
          value={form.values.monthlyLimit}
          onChange={(value) => set('monthlyLimit', value)}
          errors={form.errors}
        />
        <MonthField
          label="Ліміт діє з"
          name="monthlyLimitFrom"
          value={form.values.monthlyLimitFrom}
          onChange={(value) => set('monthlyLimitFrom', value)}
          errors={form.errors}
        />
        <AmountField
          label="План скарбнички"
          name="savingsGoal"
          required={false}
          hint="Скільки хочемо накопичити; порожньо — без плану"
          value={form.values.savingsGoal}
          onChange={(value) => set('savingsGoal', value)}
          errors={form.errors}
        />
      </Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
        <Button type="submit" variant="contained" disabled={form.isPending}>
          {form.isPending ? 'Зберігаю…' : 'Зберегти'}
        </Button>
        {saved && (
          <Typography role="status" color="text.secondary">
            Збережено
          </Typography>
        )}
      </Box>
    </Card>
  );
}
