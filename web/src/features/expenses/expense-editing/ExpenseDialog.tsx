import Box from '@mui/material/Box';
import Checkbox from '@mui/material/Checkbox';
import FormControlLabel from '@mui/material/FormControlLabel';
import FormHelperText from '@mui/material/FormHelperText';
import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';
import { useId } from 'react';
import type { ExpenseItem, Payer, PaymentMethod } from '../data/expenses-api';
import { payerLabels, paymentMethodLabels } from '../data/labels';
import { useSaveExpense } from '../data/writes-api';
import { AmountField } from '../entry-dialogs/AmountField';
import { CategoryField } from '../entry-dialogs/CategoryField';
import { EntryDialog } from '../entry-dialogs/EntryDialog';
import { fieldMessage } from '../entry-editing/entry-fields';
import { useEntryForm } from '../entry-editing/use-entry-form';
import {
  expenseFormValues,
  toExpenseRequest,
  validateExpense,
} from './expense-form';

interface ExpenseDialogProps {
  /** The expense to change; undefined adds a new one. */
  item?: ExpenseItem;
  /** The date a new expense starts with. */
  newDate: string;
  onClose: () => void;
}

/** Adds or changes an expense; «Не в ліміти» keeps it out of the limits and the carry-over. */
export function ExpenseDialog({ item, newDate, onClose }: ExpenseDialogProps) {
  const outsideHintId = useId();
  const form = useEntryForm({
    initial: expenseFormValues(item, newDate),
    validate: validateExpense,
    toBody: toExpenseRequest,
    edit: item && { id: item.id, version: item.version },
    save: useSaveExpense(),
    onSaved: onClose,
  });
  const { values, set, errors } = form;

  return (
    <EntryDialog
      title={item ? 'Змінити витрату' : 'Нова витрата'}
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
      <Box>
        <FormControlLabel
          control={
            <Checkbox
              checked={values.outsideLimits}
              onChange={(event) => set('outsideLimits', event.target.checked)}
              slotProps={{
                input: { 'aria-describedby': outsideHintId },
              }}
            />
          }
          label="Не в ліміти"
        />
        <FormHelperText id={outsideHintId} sx={{ mt: 0, mx: 0 }}>
          Враховується в підсумках, але не в лімітах і не переноситься
        </FormHelperText>
      </Box>
    </EntryDialog>
  );
}
