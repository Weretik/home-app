import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import { Link } from 'react-router-dom';
import { useShoppingSummary } from '../data/shopping-summary-api';
import { SummaryContent } from './SummaryContent';

/** «Покупки» on the home page (R-14): the figures, departments, next items, the most bought, «Открыть список». */
export function ShoppingSummary() {
  const { data, error, isPending, refetch } = useShoppingSummary();

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 } }}>
      <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
        <Button component={Link} to="/shopping">
          Открыть список
        </Button>
      </Box>
      {isPending ? (
        <Box sx={{ display: 'grid', placeItems: 'center', py: 6 }}>
          <CircularProgress aria-label="Загружаю сводку" />
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
