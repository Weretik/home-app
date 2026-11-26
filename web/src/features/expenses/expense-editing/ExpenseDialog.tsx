import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';
import {
  useExpenseCategories,
  type ExpenseItem,
  type Payer,
  type PaymentMethod,
} from '../data/expenses-api';
import { payerLabels, paymentMethodLabels } from '../data/labels';
import type { Month } from '../data/month';
import { useSaveExpense } from '../data/writes-api';
import { AmountField } from '../entry-dialogs/AmountField';
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
  month: Month;
  onClose: () => void;
}

/** Adds or changes an expense. */
export function ExpenseDialog({ item, month, onClose }: ExpenseDialogProps) {
  const categories = useExpenseCategories();
  const form = useEntryForm({
    initial: expenseFormValues(item, month),
    validate: validateExpense,
    toBody: toExpenseRequest,
    edit: item && { id: item.id, version: item.version },
    save: useSaveExpense(),
    onSaved: onClose,
  });
  const { values, set, errors } = form;

  return (
    <EntryDialog
      title={item ? 'Изменить расход' : 'Новый расход'}
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
      <TextField
        select
        label="Категория"
        required
        value={values.categoryId}
        disabled={categories.isPending}
        onChange={(event) => set('categoryId', event.target.value)}
        {...fieldMessage(errors, 'categoryId')}
      >
        {(categories.data ?? []).map((category) => (
          <MenuItem key={category.id} value={category.id}>
            {category.name}
          </MenuItem>
        ))}
      </TextField>
      <TextField
        select
        label="Кто"
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
        label="Комментарий"
        multiline
        minRows={2}
        value={values.comment}
        onChange={(event) => set('comment', event.target.value)}
        {...fieldMessage(errors, 'comment')}
      />
    </EntryDialog>
  );
}
