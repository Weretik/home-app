import Checkbox from '@mui/material/Checkbox';
import FormControl from '@mui/material/FormControl';
import FormHelperText from '@mui/material/FormHelperText';
import FormLabel from '@mui/material/FormLabel';
import ListItemText from '@mui/material/ListItemText';
import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';
import ToggleButton from '@mui/material/ToggleButton';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import type { LimitPeriod, SpendingLimitItem } from '../budget-data/budget-api';
import { useSaveSpendingLimit } from '../budget-data/budget-writes-api';
import { useExpenseCategories } from '../data/expenses-api';
import { limitPeriodLabels } from '../data/labels';
import { AmountField } from '../entry-dialogs/AmountField';
import { EntryDialog } from '../entry-dialogs/EntryDialog';
import { fieldMessage } from '../entry-editing/entry-fields';
import { useEntryForm } from '../entry-editing/use-entry-form';
import { limitFormValues, toLimitRequest, validateLimit } from './limit-form';

interface LimitDialogProps {
  /** The limit to change; undefined adds a new one. */
  item?: SpendingLimitItem;
  onClose: () => void;
}

/** Adds or changes a spending limit: name, month or quarter, amount and its categories. */
export function LimitDialog({ item, onClose }: LimitDialogProps) {
  const categories = useExpenseCategories();
  const form = useEntryForm({
    initial: limitFormValues(item),
    validate: validateLimit,
    toBody: toLimitRequest,
    edit: item && { id: item.id, version: item.version },
    save: useSaveSpendingLimit(),
    onSaved: onClose,
  });
  const { values, set, errors } = form;
  const names = new Map(
    (categories.data ?? []).map((category) => [category.id, category.name]),
  );

  return (
    <EntryDialog
      title={item ? 'Изменить лимит' : 'Новый лимит'}
      error={form.message}
      isPending={form.isPending}
      onSubmit={form.submit}
      onClose={onClose}
    >
      <TextField
        label="Название"
        required
        value={values.name}
        onChange={(event) => set('name', event.target.value)}
        {...fieldMessage(errors, 'name')}
      />
      <FormControl>
        <FormLabel id="limit-period" sx={{ mb: 1, typography: 'body2' }}>
          Период
        </FormLabel>
        <ToggleButtonGroup
          exclusive
          fullWidth
          color="primary"
          size="small"
          aria-labelledby="limit-period"
          value={values.period}
          onChange={(_, value: LimitPeriod | null) => {
            if (value) set('period', value);
          }}
        >
          {(Object.keys(limitPeriodLabels) as LimitPeriod[]).map((period) => (
            <ToggleButton
              key={period}
              value={period}
              sx={{ textTransform: 'none' }}
            >
              {limitPeriodLabels[period]}
            </ToggleButton>
          ))}
        </ToggleButtonGroup>
        {errors.period && (
          <FormHelperText error>{errors.period}</FormHelperText>
        )}
      </FormControl>
      <AmountField
        label="Сумма лимита"
        value={values.amount}
        onChange={(value) => set('amount', value)}
        errors={errors}
      />
      <TextField
        select
        label="Категории"
        required
        value={values.categoryIds}
        disabled={categories.isPending}
        onChange={(event) => {
          const value = event.target.value as unknown as string[] | string;
          set(
            'categoryIds',
            typeof value === 'string' ? value.split(',') : value,
          );
        }}
        {...fieldMessage(errors, 'categoryIds')}
        slotProps={{
          select: {
            multiple: true,
            renderValue: (selected) =>
              (selected as string[])
                .map((id) => names.get(id) ?? id)
                .join(', '),
          },
        }}
      >
        {(categories.data ?? []).map((category) => (
          <MenuItem key={category.id} value={category.id}>
            <Checkbox
              size="small"
              checked={values.categoryIds.includes(category.id)}
              sx={{ p: 0, mr: 1 }}
            />
            <ListItemText primary={category.name} />
          </MenuItem>
        ))}
      </TextField>
    </EntryDialog>
  );
}
