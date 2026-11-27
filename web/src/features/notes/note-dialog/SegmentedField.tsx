import FormControl from '@mui/material/FormControl';
import FormLabel from '@mui/material/FormLabel';
import ToggleButton from '@mui/material/ToggleButton';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import { useId } from 'react';

interface SegmentedFieldProps<T extends string> {
  label: string;
  value: T;
  options: readonly { value: T; label: string; mark?: string }[];
  onChange: (value: T) => void;
}

/** A labelled choice of a few options as one row of buttons (Про кого, Що); it fills the width. */
export function SegmentedField<T extends string>({
  label,
  value,
  options,
  onChange,
}: SegmentedFieldProps<T>) {
  const labelId = useId();
  return (
    <FormControl component="fieldset" sx={{ minWidth: 0 }}>
      <FormLabel
        id={labelId}
        component="legend"
        sx={{ mb: 0.5, typography: 'body2' }}
      >
        {label}
      </FormLabel>
      <ToggleButtonGroup
        exclusive
        fullWidth
        size="small"
        color="primary"
        aria-labelledby={labelId}
        value={value}
        onChange={(_, next: T | null) => next !== null && onChange(next)}
        sx={{
          display: 'grid',
          gap: 0.75,
          // Four options make two rows on a phone instead of four stacked ones.
          gridTemplateColumns: {
            xs: `repeat(${options.length > 3 ? 2 : options.length}, minmax(0, 1fr))`,
            sm: `repeat(${options.length}, minmax(0, 1fr))`,
          },
          // Separate buttons, each with its own border, so a wrapped row still reads as one choice.
          '& .MuiToggleButtonGroup-grouped': {
            m: 0,
            border: 1,
            borderColor: 'divider',
            borderRadius: 1,
          },
        }}
      >
        {options.map((option) => (
          <ToggleButton
            key={option.value}
            value={option.value}
            sx={{ textTransform: 'none', gap: 0.5, px: 1, minWidth: 0 }}
          >
            {option.mark && <span aria-hidden="true">{option.mark}</span>}
            {option.label}
          </ToggleButton>
        ))}
      </ToggleButtonGroup>
    </FormControl>
  );
}
