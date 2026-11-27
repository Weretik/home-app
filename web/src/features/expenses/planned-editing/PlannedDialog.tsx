import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';
import type { Payer } from '../data/expenses-api';
import { payerLabels } from '../data/labels';
import type { Month } from '../data/month';
import { AmountField } from '../entry-dialogs/AmountField';
import { CategoryField } from '../entry-dialogs/CategoryField';
import { EntryDialog } from '../entry-dialogs/EntryDialog';
import { fieldMessage } from '../entry-editing/entry-fields';
import { useEntryForm } from '../entry-editing/use-entry-form';
import type { PlannedExpenseItem } from '../planned-data/planned-api';
import { useSavePlannedExpense } from '../planned-data/planned-writes-api';
import {
  plannedFormValues,
  toPlannedRequest,
  validatePlanned,
} from './planned-form';

interface PlannedDialogProps {
  /** The plan to change; undefined adds a new one to `month`. */
  item?: PlannedExpenseItem;
  month: Month;
  onClose: () => void;
}

/** Adds or changes a planned expense: name, category, who and amount. */
export function PlannedDialog({ item, month, onClose }: PlannedDialogProps) {
  const form = useEntryForm({
    initial: plannedFormValues(item, month),
    validate: validatePlanned,
    toBody: toPlannedRequest,
    edit: item && { id: item.id, version: item.version },
    save: useSavePlannedExpense(),
    onSaved: onClose,
  });
  const { values, set, errors } = form;

  return (
    <EntryDialog
      title={item ? 'Змінити план' : 'Новий план'}
      error={form.message}
      isPending={form.isPending}
      onSubmit={form.submit}
      onClose={onClose}
    >
      <TextField
        label="Назва"
        required
        value={values.name}
        onChange={(event) => set('name', event.target.value)}
        {...fieldMessage(errors, 'name')}
      />
      <CategoryField
        value={values.categoryId}
        onChange={(value) => set('categoryId', value)}
        errors={errors}
      />
      <TextField
        select
        label="Хто"
        value={values.payer}
        onChange={(event) => set('payer', event.target.value as Payer)}
        {...fieldMessage(errors, 'payer')}
      >
        {(Object.keys(payerLabels) as Payer[]).map((payer) => (
          <MenuItem key={payer} value={payer}>
            {payerLabels[payer]}
          </MenuItem>
        ))}
      </TextField>
      <AmountField
        value={values.amount}
        onChange={(value) => set('amount', value)}
        errors={errors}
      />
    </EntryDialog>
  );
}
