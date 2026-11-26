import EventNoteOutlinedIcon from '@mui/icons-material/EventNoteOutlined';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import CircularProgress from '@mui/material/CircularProgress';
import Typography from '@mui/material/Typography';
import { Link } from 'react-router-dom';
import { useMonthSummary, type MonthSummary } from '../data/summary-api';
import { useMonthParam } from '../data/use-month-param';
import { KpiCards } from './KpiCards';
import { LimitsCard } from './LimitsCard';
import { MonthPicker } from './MonthPicker';
import { SpendingCards } from './SpendingCards';

/**
 * «Сводка расходов» on the home page: the old sheet's «Сводка» tab without its plan block. The month is in
 * `?year=&month=`, the same params the Expenses pages read, so «Открыть расходы» opens the same month.
 */
export function ExpensesSummary() {
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
          Открыть расходы
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

function SummaryContent({ summary }: { summary: MonthSummary }) {
  const isEmpty =
    summary.incomes === 0 && summary.expenses === 0 && summary.savingsNet === 0;

  return (
    <>
      <KpiCards summary={summary} />
      {isEmpty ? (
        <EmptyMonth />
      ) : (
        <>
          <LimitsCard summary={summary} />
          <SpendingCards summary={summary} />
        </>
      )}
    </>
  );
}

function EmptyMonth() {
  return (
    <Card
      sx={{
        p: { xs: 3, md: 5 },
        display: 'grid',
        justifyItems: 'center',
        textAlign: 'center',
        gap: 1,
      }}
    >
      <Box
        sx={(theme) => ({
          width: 56,
          height: 56,
          borderRadius: 3,
          display: 'grid',
          placeItems: 'center',
          color: 'primary.main',
          bgcolor: `rgba(${theme.vars.palette.primary.mainChannel} / 0.12)`,
        })}
      >
        <EventNoteOutlinedIcon />
      </Box>
      <Typography variant="h6" component="p">
        За этот месяц записей нет
      </Typography>
      <Typography color="text.secondary" sx={{ maxWidth: 360 }}>
        Лимиты, кто сколько потратил и главные категории появятся, когда в
        месяце будут доходы или расходы. Стрелками выше можно открыть другой
        месяц.
      </Typography>
    </Card>
  );
}
