import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import { Link } from 'react-router-dom';
import { useGiftsSummary } from '../data/summary-api';
import { SummaryContent } from './SummaryContent';

/** «Подарки» on the home page: figures per person, the 🔥 wishes, the nearest occasions and the «нюансы», read-only. */
export function GiftsSummary() {
  const { data, error, isPending, refetch } = useGiftsSummary();

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 } }}>
      <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
        <Button component={Link} to="/gifts">
          Открыть подарки
        </Button>
      </Box>
      {isPending ? (
        <Box sx={{ display: 'grid', placeItems: 'center', py: 6 }}>
          <CircularProgress aria-label="Загружаю сводку подарков" />
        </Box>
      ) : error ? (
        <Alert
          severity="error"
          action={
            <Button color="inherit" size="small" onClick={() => refetch()}>
              Повторить
            </Button>
          }
        >
          Не удалось загрузить сводку: {error.message}
        </Alert>
      ) : (
        <SummaryContent summary={data} />
      )}
    </Box>
  );
}
