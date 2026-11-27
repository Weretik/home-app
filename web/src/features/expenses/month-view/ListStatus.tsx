import EventNoteOutlinedIcon from '@mui/icons-material/EventNoteOutlined';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';
import { OfflineNotice } from '../../../platform/offline/status/OfflineNotice';
import { useOnline } from '../../../platform/offline/status/use-network-status';

export interface ListStatusProps {
  isPending: boolean;
  error: Error | null;
  isEmpty: boolean;
  /** A filter is on, so an empty list offers to reset it (`onResetFilters`). */
  isFiltered?: boolean;
  /** What the list shows, for the empty state's words: a month (default), a period of days or a month's plans. */
  scope?: 'month' | 'period' | 'plan';
  loadingLabel: string;
  onRetry: () => void;
  onResetFilters?: () => void;
  children: ReactNode;
}

const emptyTexts = {
  month: {
    title: 'За цей місяць записів немає',
    hint: 'Записи місяця з’являться тут. Стрілками вище можна відкрити інший місяць.',
    filteredHint: 'Скиньте фільтр або виберіть інший місяць.',
  },
  period: {
    title: 'За цей період записів немає',
    hint: 'Записи за вибрані дні з’являться тут. Дати можна змінити вище.',
    filteredHint: 'Скиньте фільтр або виберіть інші дати.',
  },
  plan: {
    title: 'На цей місяць нічого не заплановано',
    hint: 'Додайте витрати, на які чекаєте цього місяця: вони ввійдуть у прогноз і в ліміти.',
    filteredHint: 'Скиньте фільтр або виберіть інший місяць.',
  },
} as const;

/** Loading, error and empty states of a month list (docs/design.md: designed, not blank); the list otherwise. */
export function ListStatus({
  isPending,
  error,
  isEmpty,
  isFiltered = false,
  scope = 'month',
  loadingLabel,
  onRetry,
  onResetFilters,
  children,
}: ListStatusProps) {
  const online = useOnline();
  if (isPending && !online) return <OfflineNotice />;
  if (isPending)
    return (
      <Box sx={{ display: 'grid', placeItems: 'center', py: 6 }}>
        <CircularProgress aria-label={loadingLabel} />
      </Box>
    );

  if (error)
    return (
      <Box sx={{ p: { xs: 2, md: 3 } }}>
        <Alert
          severity="error"
          action={
            <Button color="inherit" size="small" onClick={onRetry}>
              Повторити
            </Button>
          }
        >
          Не вдалося завантажити: {error.message}
        </Alert>
      </Box>
    );

  if (isEmpty)
    return (
      <Box
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
          {isFiltered
            ? 'З цим фільтром записів немає'
            : emptyTexts[scope].title}
        </Typography>
        <Typography color="text.secondary" sx={{ maxWidth: 360 }}>
          {isFiltered ? emptyTexts[scope].filteredHint : emptyTexts[scope].hint}
        </Typography>
        {isFiltered && (
          <Button onClick={onResetFilters} sx={{ mt: 1 }}>
            Скинути фільтр
          </Button>
        )}
      </Box>
    );

  return children;
}
