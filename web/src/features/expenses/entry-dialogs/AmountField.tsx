import InputAdornment from '@mui/material/InputAdornment';
import TextField from '@mui/material/TextField';
import { fieldMessage, type FieldErrors } from '../entry-editing/entry-fields';

interface AmountFieldProps {
  value: string;
  onChange: (value: string) => void;
  errors: FieldErrors;
  label?: string;
  /** The contract's field name, whose message shows under the field. */
  name?: string;
  required?: boolean;
  /** A hint under the field while it has no message. */
  hint?: string;
}

/** An amount in hryvnias; the phone shows the decimal keypad. */
export function AmountField({
  value,
  onChange,
  errors,
  label = 'Сума',
  name = 'amount',
  required = true,
  hint,
}: AmountFieldProps) {
  const message = fieldMessage(errors, name);
  return (
    <TextField
      label={label}
      required={required}
      value={value}
      onChange={(event) => onChange(event.target.value)}
      error={message.error}
      helperText={message.helperText ?? hint}
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
