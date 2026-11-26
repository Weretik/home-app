import InputAdornment from '@mui/material/InputAdornment';
import TextField from '@mui/material/TextField';

interface PriceFieldProps {
  label: string;
  value: string;
  error?: string;
  onChange: (value: string) => void;
  size?: 'small' | 'medium';
}

/** An optional price in hryvnias; the phone shows the decimal keypad. Empty means the price is unknown. */
export function PriceField({
  label,
  value,
  error,
  onChange,
  size,
}: PriceFieldProps) {
  return (
    <TextField
      label={label}
      value={value}
      size={size}
      onChange={(event) => onChange(event.target.value)}
      error={Boolean(error)}
      helperText={error}
      slotProps={{
        htmlInput: { inputMode: 'decimal', autoComplete: 'off' },
        input: {
          endAdornment: <InputAdornment position="end">₴</InputAdornment>,
        },
      }}
    />
  );
}
