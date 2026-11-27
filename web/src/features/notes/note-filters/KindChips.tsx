import Box from '@mui/material/Box';
import Chip from '@mui/material/Chip';
import {
  kindFilterLabels,
  kindFilters,
  type KindFilter,
} from '../url-state/filters';

const options: { value: KindFilter | undefined; label: string }[] = [
  { value: undefined, label: 'Усе' },
  ...kindFilters.map((value) => ({ value, label: kindFilterLabels[value] })),
];

/** «Усе · Любить · Не любить · Факти», one chip pressed (FR-002). */
export function KindChips({
  value,
  onChange,
}: {
  value: KindFilter | undefined;
  onChange: (value: KindFilter | undefined) => void;
}) {
  return (
    <Box
      role="group"
      aria-label="Що показати"
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
