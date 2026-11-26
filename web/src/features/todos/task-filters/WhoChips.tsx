import Box from '@mui/material/Box';
import Chip from '@mui/material/Chip';
import { whoLabels, whoValues, type Who } from '../url-state/filters';

const options: { value: Who | undefined; label: string }[] = [
  { value: undefined, label: 'Все' },
  ...whoValues.map((value) => ({ value, label: whoLabels[value] })),
];

/** «Кому»: Все · Маша · Виталий · Общее, one chip pressed. */
export function WhoChips({
  value,
  onChange,
}: {
  value: Who | undefined;
  onChange: (value: Who | undefined) => void;
}) {
  return (
    <Box
      role="group"
      aria-label="Кому"
      sx={{ display: 'flex', gap: 0.75, flexWrap: 'wrap' }}
    >
      {options.map((option) => {
        const selected = option.value === value;
        return (
          <Chip
            key={option.label}
            label={option.label}
            clickable
            color={selected ? 'primary' : 'default'}
            variant={selected ? 'filled' : 'outlined'}
            aria-pressed={selected}
            onClick={() => onChange(option.value)}
          />
        );
      })}
    </Box>
  );
}
