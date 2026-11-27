import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import { Link } from 'react-router-dom';
import { useMonthSummary } from '../data/summary-api';
import { useMonthParam } from '../data/use-month-param';
import { MonthPicker } from './MonthPicker';
import { SummaryContent } from './SummaryContent';
import { OfflineNotice } from '../../../../platform/offline/status/OfflineNotice';
import { useOnline } from '../../../../platform/offline/status/use-network-status';

/**
 * «Зведення» of «Бюджет» on the home page: the old sheet's «Сводка» tab, with the forecast from the planned expenses. The month is in
 * `?year=&month=`, the same params the Expenses pages read, so «Відкрити витрати» opens the same month.
 */
export function ExpensesSummary() {
  const online = useOnline();
  const [month, setMonth] = useMonthParam();
  const { data, error, isPending, refetch } = useMonthSummary(month);

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 } }}>
      <Box
        sx={{
          display: 'flex',
          flexWrap: 'wrap',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 1,
        }}
      >
        <MonthPicker month={month} onChange={setMonth} />
        <Button
          component={Link}
          to={`/expenses?year=${month.year}&month=${month.month}`}
        >
          Відкрити витрати
        </Button>
      </Box>
      {isPending && !online ? (
        <OfflineNotice compact />
      ) : isPending ? (
        <Box sx={{ display: 'grid', placeItems: 'center', py: 6 }}>
          <CircularProgress aria-label="Завантажую зведення" />
        </Box>
      ) : error ? (
        <Alert
          severity="error"
          action={
            <Button color="inherit" size="small" onClick={() => refetch()}>
              Повторити
            </Button>
          }
        >
          Не вдалося завантажити зведення: {error.message}
        </Alert>
      ) : (
        <SummaryContent summary={data} />
      )}
    </Box>
  );
}
