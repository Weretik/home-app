import ChevronLeftOutlinedIcon from '@mui/icons-material/ChevronLeftOutlined';
import ChevronRightOutlinedIcon from '@mui/icons-material/ChevronRightOutlined';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import { useLocation } from 'react-router-dom';
import { currentMonth, formatMonth, shiftMonth } from '../data/month';
import { useMonthParam } from '../url-state/use-month-param';
import { usePeriodParam } from '../url-state/use-period-param';
import { sectionPath, settingsPath } from './expenses-pages';

/**
 * Previous / next month arrows around the month name; «К текущему» when another month is open. Hidden while the
 * «Расходы» page shows a period of days instead of a month, and on «Настройки», which has no month.
 */
export function MonthPicker() {
  const [month, setMonth] = useMonthParam();
  const { active: periodActive } = usePeriodParam();
  const { pathname } = useLocation();
  const today = currentMonth();
  const isCurrent = month.year === today.year && month.month === today.month;

  const path = pathname.replace(/\/$/, '');
  if (path === settingsPath) return null;
  // Only the expense list (the section's index page) reads a period; the other pages stay on the month.
  if (periodActive && path === sectionPath) return null;

  return (
    <Box
      role="group"
      aria-label="Месяц"
      sx={{ display: 'flex', alignItems: 'center', gap: 0.5, flexWrap: 'wrap' }}
    >
      <Tooltip title="Предыдущий месяц">
        <IconButton
          aria-label="Предыдущий месяц"
          onClick={() => setMonth(shiftMonth(month, -1))}
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
          onClick={() => setMonth(shiftMonth(month, 1))}
        >
          <ChevronRightOutlinedIcon />
        </IconButton>
      </Tooltip>
      {!isCurrent && (
        <Button size="small" onClick={() => setMonth(today)}>
          К текущему
        </Button>
      )}
    </Box>
  );
}
