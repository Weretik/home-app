import Box from '@mui/material/Box';
import Chip from '@mui/material/Chip';
import TextField from '@mui/material/TextField';
import { addDays, kyivToday, weekendOf } from '../data/due-date';

/** The due date: a date field and the quick chips «Сегодня», «Завтра», «На выходных», «Без срока» (FR-005). */
export function DueDateField({
  value,
  error,
  onChange,
}: {
  value: string;
  error?: string;
  onChange: (value: string) => void;
}) {
  const today = kyivToday();
  const quick = [
    { label: 'Сегодня', day: today },
    { label: 'Завтра', day: addDays(today, 1) },
    { label: 'На выходных', day: weekendOf(today) },
    { label: 'Без срока', day: '' },
  ];

  return (
    <Box sx={{ display: 'grid', gap: 1 }}>
      <TextField
        label="Срок"
        type="date"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        error={Boolean(error)}
        helperText={error}
        slotProps={{ inputLabel: { shrink: true } }}
      />
      <Box
        role="group"
        aria-label="Быстрый срок"
        sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.75 }}
      >
        {quick.map(({ label, day }) => {
          const selected = value === day;
          return (
            <Chip
              key={label}
              label={label}
              size="small"
              clickable
              color={selected ? 'primary' : 'default'}
              variant={selected ? 'filled' : 'outlined'}
              aria-pressed={selected}
              onClick={() => onChange(day)}
            />
          );
        })}
      </Box>
    </Box>
  );
}
