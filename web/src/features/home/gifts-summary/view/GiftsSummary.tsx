import CardGiftcardOutlinedIcon from '@mui/icons-material/CardGiftcardOutlined';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import CircularProgress from '@mui/material/CircularProgress';
import Typography from '@mui/material/Typography';
import { Link } from 'react-router-dom';
import { useGiftsSummary, type GiftsSummaryView } from '../data/summary-api';
import { DetailsCard } from './DetailsCard';
import { OccasionsCard } from './OccasionsCard';
import { RecipientCards } from './RecipientCards';
import { TopWishesCard } from './TopWishesCard';

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

function SummaryContent({ summary }: { summary: GiftsSummaryView }) {
  return (
    <>
      <RecipientCards summary={summary} />
      <Box
        sx={{
          display: 'grid',
          gap: { xs: 2, md: 3 },
          gridTemplateColumns: {
            xs: '1fr',
            md: '1fr 1fr',
            lg: 'repeat(3, 1fr)',
          },
          alignItems: 'start',
        }}
      >
        {summary.total.wantedCount === 0 ? (
          <NoWishes />
        ) : (
          <>
            <TopWishesCard summary={summary} />
            <OccasionsCard summary={summary} />
          </>
        )}
        <DetailsCard summary={summary} />
      </Box>
    </>
  );
}

function NoWishes() {
  return (
    <Card
      sx={{
        p: 3,
        display: 'grid',
        justifyItems: 'center',
        textAlign: 'center',
        gap: 1,
      }}
    >
      <CardGiftcardOutlinedIcon color="primary" />
      <Typography variant="h6" component="p">
        Желаний пока нет
      </Typography>
      <Typography color="text.secondary">
        Добавьте подарки в разделе «Подарки».
      </Typography>
      <Button component={Link} to="/gifts">
        Открыть подарки
      </Button>
    </Card>
  );
}
