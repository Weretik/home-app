import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';
import type { TaskView } from '../data/todos-api';
import { whenLabels, whenOptions, whenTitles } from '../url-state/date-filter';
import type { useTodoFilters } from '../url-state/use-todo-filters';

const fieldSx = { minWidth: 0, flex: { xs: '1 1 100%', sm: '0 1 180px' } };

/**
 * «Термін» on the active list, «Коли зроблено» in the archive (FR-002, FR-006): a select of quick choices and, for
 * «Період», two day fields «З» and «По» (either may stay empty).
 */
export function DateFilter({
  view,
  state,
}: {
  view: TaskView;
  state: ReturnType<typeof useTodoFilters>;
}) {
  const { when, from, to } = state.filters;
  const day = (
    label: string,
    value: string | undefined,
    onChange: (value: string | undefined) => void,
  ) => (
    <TextField
      size="small"
      type="date"
      label={label}
      value={value ?? ''}
      onChange={(event) => onChange(event.target.value || undefined)}
      sx={{ ...fieldSx, flex: { xs: '1 1 40%', sm: '0 1 160px' } }}
      slotProps={{ inputLabel: { shrink: true } }}
    />
  );

  return (
    <>
      <TextField
        select
        size="small"
        label={whenTitles[view]}
        value={when ?? ''}
        onChange={(event) =>
          state.setWhen((event.target.value || undefined) as typeof when)
        }
        sx={fieldSx}
        slotProps={{
          select: { displayEmpty: true },
          inputLabel: { shrink: true },
        }}
      >
        <MenuItem value="">Будь-який</MenuItem>
        {whenOptions[view].map((option) => (
          <MenuItem key={option} value={option}>
            {whenLabels[option]}
          </MenuItem>
        ))}
      </TextField>
      {when === 'period' && (
        <>
          {day('З', from, (value) => state.setPeriod(value, to))}
          {day('По', to, (value) => state.setPeriod(from, value))}
        </>
      )}
    </>
  );
}
