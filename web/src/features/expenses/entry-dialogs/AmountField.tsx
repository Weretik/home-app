import InputAdornment from '@mui/material/InputAdornment';
import TextField from '@mui/material/TextField';
import { fieldMessage, type FieldErrors } from '../entry-editing/entry-fields';

interface AmountFieldProps {
  value: string;
  onChange: (value: string) => void;
  errors: FieldErrors;
  label?: string;
}

/** A positive amount in hryvnias; the phone shows the decimal keypad. */
export function AmountField({
  value,
  onChange,
  errors,
  label = 'Сумма',
}: AmountFieldProps) {
  return (
    <TextField
      label={label}
      required
      value={value}
      onChange={(event) => onChange(event.target.value)}
      {...fieldMessage(errors, 'amount')}
      slotProps={{
        htmlInput: {
          inputMode: 'decimal',
          autoComplete: 'off',
          sx: { fontVariantNumeric: 'tabular-nums' },
        },
        input: {
          endAdornment: <InputAdornment position="end">₴</InputAdornment>,
        },
      }}
    />
  );
}
