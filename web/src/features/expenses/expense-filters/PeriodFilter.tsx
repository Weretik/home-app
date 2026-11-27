import DateRangeOutlinedIcon from '@mui/icons-material/DateRangeOutlined';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import FormHelperText from '@mui/material/FormHelperText';
import TextField from '@mui/material/TextField';
import type { Month } from '../data/month';
import type { PeriodParam } from '../url-state/use-period-param';

interface PeriodFilterProps {
  param: PeriodParam;
  /** The open month, the period's first value. */
  month: Month;
  /** Why the period cannot be shown; undefined when it is fine. */
  error: string | undefined;
}

/**
 * «Період»: shows the expenses of any days from–to instead of the month. Off, it is one button; on, two date fields
 * («з», «по») with the reason under them when the period is wrong, and «До місяця» to go back.
 */
export function PeriodFilter({ param, month, error }: PeriodFilterProps) {
  if (!param.active)
    return (
      <Button
        variant="outlined"
        startIcon={<DateRangeOutlinedIcon />}
        onClick={() => param.start(month)}
      >
        Період
      </Button>
    );

  const errorId = 'expense-period-error';
  const dateField = (edge: 'from' | 'to', label: string) => (
    <TextField
      label={label}
      type="date"
      size="small"
      value={param.period[edge]}
      onChange={(event) => param.setEdge(edge, event.target.value)}
      error={Boolean(error)}
      slotProps={{
        inputLabel: { shrink: true },
        htmlInput: { 'aria-describedby': error ? errorId : undefined },
      }}
    />
  );

  return (
    <Box
      role="group"
      aria-label="Період"
      sx={{
        display: 'grid',
        gap: 2,
        gridTemplateColumns: { xs: '1fr 1fr', sm: 'auto auto auto' },
        alignItems: 'center',
        justifyContent: 'start',
      }}
    >
      {dateField('from', 'з')}
      {dateField('to', 'по')}
      <Button
        size="small"
        onClick={param.end}
        sx={{ gridColumn: { xs: '1 / -1', sm: 'auto' }, justifySelf: 'start' }}
      >
        До місяця
      </Button>
      {error && (
        <FormHelperText
          id={errorId}
          error
          role="alert"
          sx={{ gridColumn: '1 / -1', mx: 0, mt: -1 }}
        >
          {error}
        </FormHelperText>
      )}
    </Box>
  );
}
