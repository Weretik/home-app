import TextField from '@mui/material/TextField';
import type { IncomeItem } from '../data/expenses-api';
import type { Month } from '../data/month';
import { useSaveIncome } from '../data/writes-api';
import { AmountField } from '../entry-dialogs/AmountField';
import { EntryDialog } from '../entry-dialogs/EntryDialog';
import { PersonField } from '../entry-dialogs/PersonField';
import { fieldMessage } from '../entry-editing/entry-fields';
import { useEntryForm } from '../entry-editing/use-entry-form';
import {
  incomeFormValues,
  toIncomeRequest,
  validateIncome,
} from './income-form';

interface IncomeDialogProps {
  /** The income to change; undefined adds a new one. */
  item?: IncomeItem;
  month: Month;
  onClose: () => void;
}

/** Adds or changes an income. */
export function IncomeDialog({ item, month, onClose }: IncomeDialogProps) {
  const form = useEntryForm({
    initial: incomeFormValues(item, month),
    validate: validateIncome,
    toBody: toIncomeRequest,
    edit: item && { id: item.id, version: item.version },
    save: useSaveIncome(),
    onSaved: onClose,
  });
  const { values, set, errors } = form;

  return (
    <EntryDialog
      title={item ? 'Змінити дохід' : 'Новий дохід'}
      error={form.message}
      isPending={form.isPending}
      onSubmit={form.submit}
      onClose={onClose}
    >
      <TextField
        label="Дата"
        type="date"
        required
        value={values.date}
        onChange={(event) => set('date', event.target.value)}
        {...fieldMessage(errors, 'date')}
        slotProps={{ inputLabel: { shrink: true } }}
      />
      <PersonField
        value={values.person}
        onChange={(value) => set('person', value)}
        errors={errors}
      />
      <AmountField
        value={values.amount}
        onChange={(value) => set('amount', value)}
        errors={errors}
      />
      <TextField
        label="Коментар"
        multiline
        minRows={2}
        value={values.comment}
        onChange={(event) => set('comment', event.target.value)}
        {...fieldMessage(errors, 'comment')}
      />
    </EntryDialog>
  );
}
