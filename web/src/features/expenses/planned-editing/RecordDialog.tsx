import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import type { PaymentMethod } from '../data/expenses-api';
import { paymentMethodLabels } from '../data/labels';
import { EntryDialog } from '../entry-dialogs/EntryDialog';
import { fieldMessage } from '../entry-editing/entry-fields';
import { useEntryForm } from '../entry-editing/use-entry-form';
import type { PlannedExpenseItem } from '../planned-data/planned-api';
import { useRecordPlannedExpense } from '../planned-data/planned-writes-api';
import {
  recordFormValues,
  toRecordRequest,
  validateRecord,
} from './record-form';

interface RecordDialogProps {
  item: PlannedExpenseItem;
  /** The plan in one line: «Страховка · Машина · Віталій · 4 200,00 ₴». */
  summary: string;
  onClose: () => void;
}

/**
 * «Записати як витрату»: the plan becomes an expense with its category, payer and amount on the chosen day, and the
 * plan is gone. Sent with the plan's version in If-Match.
 */
export function RecordDialog({ item, summary, onClose }: RecordDialogProps) {
  const form = useEntryForm({
    initial: recordFormValues(),
    validate: validateRecord,
    toBody: toRecordRequest,
    save: useRecordPlannedExpense(item),
    onSaved: onClose,
  });
  const { values, set, errors } = form;

  return (
    <EntryDialog
      title="Записати як витрату"
      error={form.message}
      isPending={form.isPending}
      onSubmit={form.submit}
      onClose={onClose}
      submitLabel="Записати"
      pendingLabel="Записую…"
    >
      <Typography sx={{ fontWeight: 600, overflowWrap: 'anywhere' }}>
        {summary}
      </Typography>
      <TextField
        label="Дата"
        type="date"
        required
        value={values.date}
        onChange={(event) => set('date', event.target.value)}
        {...fieldMessage(errors, 'date')}
        slotProps={{ inputLabel: { shrink: true } }}
      />
      <TextField
        select
        label="Оплата"
        value={values.paymentMethod}
        onChange={(event) =>
          set('paymentMethod', event.target.value as PaymentMethod)
        }
        {...fieldMessage(errors, 'paymentMethod')}
      >
        {(Object.keys(paymentMethodLabels) as PaymentMethod[]).map((method) => (
          <MenuItem key={method} value={method}>
            {paymentMethodLabels[method]}
          </MenuItem>
        ))}
      </TextField>
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
