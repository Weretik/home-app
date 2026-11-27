import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import { Link } from 'react-router-dom';
import { useShoppingSummary } from '../data/shopping-summary-api';
import { SummaryContent } from './SummaryContent';
import { OfflineNotice } from '../../../../platform/offline/status/OfflineNotice';
import { useOnline } from '../../../../platform/offline/status/use-network-status';

/** «Покупки» on the home page (R-14): the figures, departments, next items, the most bought, «Відкрити список». */
export function ShoppingSummary() {
  const online = useOnline();
  const { data, error, isPending, refetch } = useShoppingSummary();

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 } }}>
      <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
        <Button component={Link} to="/shopping">
          Відкрити список
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
