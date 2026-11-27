import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import List from '@mui/material/List';
import Skeleton from '@mui/material/Skeleton';
import Typography from '@mui/material/Typography';
import { useId, useState } from 'react';
import type { Month } from '../data/month';
import type { SavingsMonthSummary } from '../data/savings-api';
import { SavingsMonthRow } from './SavingsMonthRow';

/** Months shown before «Показати всі». */
const FIRST_MONTHS = 12;

interface SavingsMonthsCardProps {
  /** Newest first; undefined while loading. */
  months?: SavingsMonthSummary[];
  error: Error | null;
  open: Month;
  onOpen: (month: Month) => void;
  onRetry: () => void;
}

/** «По місяцях»: each month's put in, taken out and closing balance; a month opens its movements. */
export function SavingsMonthsCard({
  months,
  error,
  open,
  onOpen,
  onRetry,
}: SavingsMonthsCardProps) {
  const titleId = useId();
  const [showAll, setShowAll] = useState(false);
  const shown = showAll ? months : months?.slice(0, FIRST_MONTHS);

  return (
    <Card component="section" aria-labelledby={titleId}>
      <Typography
        id={titleId}
        variant="subtitle1"
        component="h2"
        sx={{ fontWeight: 600, px: 2, pt: 2, pb: 1 }}
      >
        По місяцях
      </Typography>
      {error ? (
        <Box sx={{ p: 2 }}>
          <Alert
            severity="error"
            action={
              <Button color="inherit" size="small" onClick={onRetry}>
                Повторити
              </Button>
            }
          >
            Не вдалося завантажити: {error.message}
          </Alert>
        </Box>
      ) : !shown ? (
        <Box sx={{ px: 2, pb: 2 }}>
          <Skeleton height={48} />
          <Skeleton height={48} />
        </Box>
      ) : shown.length === 0 ? (
        <Typography color="text.secondary" sx={{ px: 2, pb: 2 }}>
          Операцій зі скарбничкою ще немає. Додайте першу — вона з’явиться тут.
        </Typography>
      ) : (
        <List disablePadding aria-label="Скарбничка по місяцях">
          {shown.map((month) => (
            <SavingsMonthRow
              key={`${month.year}-${month.month}`}
              month={month}
              isOpen={month.year === open.year && month.month === open.month}
              onOpen={onOpen}
            />
          ))}
        </List>
      )}
      {!showAll && months && months.length > FIRST_MONTHS && (
        <Box sx={{ p: 2, borderTop: 1, borderColor: 'divider' }}>
          <Button variant="outlined" onClick={() => setShowAll(true)}>
            Показати всі місяці
          </Button>
        </Box>
      )}
    </Card>
  );
}
