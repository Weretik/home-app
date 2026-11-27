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
import { OfflineNotice } from '../../../../platform/offline/status/OfflineNotice';
import { useOnline } from '../../../../platform/offline/status/use-network-status';

/**
 * «Справи» on the home page (FR-008): per assignee the active, overdue and due-today counts, done in the last week,
 * «На сьогодні» (overdue, today's and urgent tasks, each ticks done in place; the card is shown only when there are
 * such tasks), and «Відкрити справи».
 */
export function TodosSummary() {
  const online = useOnline();
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
          {data ? `Зроблено за тиждень: ${data.doneLast7Days}` : ' '}
        </Typography>
        <Button component={Link} to="/todos">
          Відкрити справи
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
                  На сьогодні
                </Typography>
                <Button component={Link} to="/todos?when=today" size="small">
                  Усі на сьогодні
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
