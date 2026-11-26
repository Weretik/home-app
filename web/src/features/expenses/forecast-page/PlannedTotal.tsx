import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { formatAmount, formatRecordCount } from '../data/formatting';

interface PlannedTotalProps {
  /** The month's plans; undefined while loading or after an error («—»). */
  plans?: { totalAmount: number; count: number };
}

/** «Запланировано 12 000,00 ₴» and how many plans the month has; announced when it changes. */
export function PlannedTotal({ plans }: PlannedTotalProps) {
  return (
    <Box role="status" aria-live="polite" sx={{ minWidth: 0 }}>
      <Typography variant="subtitle1" component="h2" sx={{ fontWeight: 600 }}>
        Запланировано{' '}
        <Box
          component="span"
          sx={{ whiteSpace: 'nowrap', fontVariantNumeric: 'tabular-nums' }}
        >
          {plans ? formatAmount(plans.totalAmount) : '—'}
        </Box>
      </Typography>
      {plans && (
        <Typography variant="body2" color="text.secondary">
          {formatRecordCount(plans.count)} на месяц · входят в прогноз и лимиты
        </Typography>
      )}
    </Box>
  );
}
