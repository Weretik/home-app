import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import CircularProgress from '@mui/material/CircularProgress';
import Typography from '@mui/material/Typography';
import { Link } from 'react-router-dom';
import { useTodoSummary } from '../data/todos-summary-api';
import { AssigneeCards } from './AssigneeCards';
import { FocusList } from './FocusList';

/**
 * «Дела» on the home page (FR-008): per assignee the active, overdue and due-today counts, done in the last week,
 * «На сегодня» (overdue, today's and urgent tasks, each ticks done in place; the card is shown only when there are
 * such tasks), and «Открыть дела».
 */
export function TodosSummary() {
  const { data, error, isPending, refetch } = useTodoSummary();

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
        <Typography
          color="text.secondary"
          sx={{ fontVariantNumeric: 'tabular-nums' }}
        >
          {data ? `Сделано за неделю: ${data.doneLast7Days}` : ' '}
        </Typography>
        <Button component={Link} to="/todos">
          Открыть дела
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
        <>
          <AssigneeCards counts={data.assignees} />
          {data.focus.length > 0 && (
            <Card component="section" aria-labelledby="todos-focus-title">
              <Box
                sx={{
                  display: 'flex',
                  flexWrap: 'wrap',
                  alignItems: 'baseline',
                  justifyContent: 'space-between',
                  columnGap: 1,
                  px: 2,
                  pt: 2,
                  pb: 1,
                }}
              >
                <Typography
                  id="todos-focus-title"
                  variant="subtitle1"
                  component="h2"
                  sx={{ fontWeight: 600 }}
                >
                  На сегодня
                </Typography>
                <Button component={Link} to="/todos?when=today" size="small">
                  Все на сегодня
                </Button>
              </Box>
              <FocusList tasks={data.focus} today={data.today} />
            </Card>
          )}
        </>
      )}
    </Box>
  );
}
