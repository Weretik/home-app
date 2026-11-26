import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';

interface FilterSelectProps<T extends string> {
  label: string;
  value: T | undefined;
  options: readonly { value: T; label: string }[];
  onChange: (value: T | undefined) => void;
  disabled?: boolean;
}

/** A list filter: a select whose first option «Все» turns it off. */
export function FilterSelect<T extends string>({
  label,
  value,
  options,
  onChange,
  disabled,
}: FilterSelectProps<T>) {
  return (
    <TextField
      select
      size="small"
      label={label}
      value={value ?? ''}
      disabled={disabled}
      onChange={(event) =>
        onChange(event.target.value ? (event.target.value as T) : undefined)
      }
      sx={{ minWidth: { xs: 0, sm: 200 } }}
      slotProps={{
        select: { displayEmpty: true },
        inputLabel: { shrink: true },
      }}
    >
      <MenuItem value="">Все</MenuItem>
      {options.map((option) => (
        <MenuItem key={option.value} value={option.value}>
          {option.label}
        </MenuItem>
      ))}
    </TextField>
  );
}
