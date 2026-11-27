import FormControl from '@mui/material/FormControl';
import FormControlLabel from '@mui/material/FormControlLabel';
import FormLabel from '@mui/material/FormLabel';
import Radio from '@mui/material/Radio';
import RadioGroup from '@mui/material/RadioGroup';
import TextField from '@mui/material/TextField';
import { useId } from 'react';
import type { Month } from '../data/month';
import type {
  SavingsDirection,
  SavingsMovementItem,
} from '../data/savings-api';
import { useSaveSavingsMovement } from '../data/writes-api';
import { AmountField } from '../entry-dialogs/AmountField';
import { EntryDialog } from '../entry-dialogs/EntryDialog';
import { PersonField } from '../entry-dialogs/PersonField';
import { fieldMessage } from '../entry-editing/entry-fields';
import { useEntryForm } from '../entry-editing/use-entry-form';
import {
  directionLabels,
  savingsFormValues,
  toSavingsRequest,
  validateSavings,
} from './savings-form';

interface SavingsDialogProps {
  /** The movement to change; undefined adds a new one. */
  item?: SavingsMovementItem;
  month: Month;
  onClose: () => void;
}

/** Adds or changes a movement of the jar: put in or taken out, and a positive amount. */
export function SavingsDialog({ item, month, onClose }: SavingsDialogProps) {
  const directionId = useId();
  const form = useEntryForm({
    initial: savingsFormValues(item, month),
    validate: validateSavings,
    toBody: toSavingsRequest,
    edit: item && { id: item.id, version: item.version },
    save: useSaveSavingsMovement(),
    onSaved: onClose,
  });
  const { values, set, errors } = form;

  return (
    <EntryDialog
      title={item ? 'Змінити операцію' : 'Скарбничка: нова операція'}
      error={form.message}
      isPending={form.isPending}
      onSubmit={form.submit}
      onClose={onClose}
    >
      <FormControl>
        <FormLabel id={directionId}>Що зробили</FormLabel>
        <RadioGroup
          aria-labelledby={directionId}
          value={values.direction}
          onChange={(event) =>
            set('direction', event.target.value as SavingsDirection)
          }
        >
          {(Object.keys(directionLabels) as SavingsDirection[]).map(
            (direction) => (
              <FormControlLabel
                key={direction}
                value={direction}
                control={<Radio />}
                label={directionLabels[direction]}
              />
            ),
          )}
        </RadioGroup>
      </FormControl>
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
