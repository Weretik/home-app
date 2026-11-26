import ChevronLeftOutlinedIcon from '@mui/icons-material/ChevronLeftOutlined';
import ChevronRightOutlinedIcon from '@mui/icons-material/ChevronRightOutlined';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import {
  currentMonth,
  formatMonth,
  shiftMonth,
  type Month,
} from '../data/month';

/** Previous / next month arrows around the month name; «К текущему» when another month is open. */
export function MonthPicker({
  month,
  onChange,
}: {
  month: Month;
  onChange: (month: Month) => void;
}) {
  const today = currentMonth();
  const isCurrent = month.year === today.year && month.month === today.month;

  return (
    <Box
      role="group"
      aria-label="Месяц"
      sx={{ display: 'flex', alignItems: 'center', gap: 0.5, flexWrap: 'wrap' }}
    >
      <Tooltip title="Предыдущий месяц">
        <IconButton
          aria-label="Предыдущий месяц"
          onClick={() => onChange(shiftMonth(month, -1))}
        >
          <ChevronLeftOutlinedIcon />
        </IconButton>
      </Tooltip>
      <Typography
        aria-live="polite"
        sx={{
          minWidth: 136,
          textAlign: 'center',
          fontWeight: 600,
          fontVariantNumeric: 'tabular-nums',
        }}
      >
        {formatMonth(month)}
      </Typography>
      <Tooltip title="Следующий месяц">
        <IconButton
          aria-label="Следующий месяц"
          onClick={() => onChange(shiftMonth(month, 1))}
        >
          <ChevronRightOutlinedIcon />
        </IconButton>
      </Tooltip>
      {!isCurrent && (
        <Button size="small" onClick={() => onChange(today)}>
          К текущему
        </Button>
      )}
    </Box>
  );
}
