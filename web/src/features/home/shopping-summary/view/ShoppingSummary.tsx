import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import CircularProgress from '@mui/material/CircularProgress';
import Typography from '@mui/material/Typography';
import { Link } from 'react-router-dom';
import {
  useShoppingSummary,
  type ShoppingSummary as Summary,
} from '../data/shopping-summary-api';
import { DepartmentsCard } from './DepartmentsCard';
import { FrequentCard } from './FrequentCard';
import { NextItemsCard } from './NextItemsCard';
import { SummaryKpis } from './SummaryKpis';

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

function SummaryContent({ summary }: { summary: Summary }) {
  return (
    <>
      <SummaryKpis summary={summary} />
      {summary.neededCount === 0 ? (
        <Card
          sx={{
            p: { xs: 3, md: 5 },
            display: 'grid',
            justifyItems: 'center',
            textAlign: 'center',
            gap: 1,
          }}
        >
          <Typography variant="h6" component="p">
            Список пуст 🎉
          </Typography>
          <Typography color="text.secondary" sx={{ maxWidth: 360 }}>
            Всё куплено. Новое добавляется в разделе «Покупки».
          </Typography>
        </Card>
      ) : (
        <Box
          sx={{
            display: 'grid',
            gap: { xs: 2, md: 3 },
            gridTemplateColumns: {
              xs: '1fr',
              md: 'repeat(2, 1fr)',
              lg: 'repeat(3, 1fr)',
            },
            alignItems: 'start',
          }}
        >
          <NextItemsCard summary={summary} />
          <DepartmentsCard summary={summary} />
          <FrequentCard summary={summary} />
        </Box>
      )}
    </>
  );
}
