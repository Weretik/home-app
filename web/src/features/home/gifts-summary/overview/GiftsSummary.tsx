import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import { Link } from 'react-router-dom';
import { useGiftsSummary } from '../data/summary-api';
import { SummaryContent } from './SummaryContent';
import { OfflineNotice } from '../../../../platform/offline/status/OfflineNotice';
import { useOnline } from '../../../../platform/offline/status/use-network-status';

/** «Подарунки» on the home page: figures per person, the 🔥 wishes, the nearest occasions and the «нюанси», read-only. */
export function GiftsSummary() {
  const online = useOnline();
  const { data, error, isPending, refetch } = useGiftsSummary();

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 } }}>
      <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
        <Button component={Link} to="/gifts">
          Відкрити подарунки
        </Button>
      </Box>
      {isPending && !online ? (
        <OfflineNotice compact />
      ) : isPending ? (
        <Box sx={{ display: 'grid', placeItems: 'center', py: 6 }}>
          <CircularProgress aria-label="Завантажую зведення подарунків" />
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
