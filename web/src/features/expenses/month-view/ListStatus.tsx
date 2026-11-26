import EventNoteOutlinedIcon from '@mui/icons-material/EventNoteOutlined';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';

interface ListStatusProps {
  isPending: boolean;
  error: Error | null;
  isEmpty: boolean;
  /** A filter is on, so an empty list offers to reset it. */
  isFiltered: boolean;
  /** What the list shows, for the empty state's words: a month (default) or a period of days. */
  scope?: 'month' | 'period';
  loadingLabel: string;
  onRetry: () => void;
  onResetFilters: () => void;
  children: ReactNode;
}

const emptyTexts = {
  month: {
    title: 'За этот месяц записей нет',
    hint: 'Записи месяца появятся здесь. Стрелками выше можно открыть другой месяц.',
    filteredHint: 'Сбросьте фильтр или выберите другой месяц.',
  },
  period: {
    title: 'За этот период записей нет',
    hint: 'Записи за выбранные дни появятся здесь. Даты можно изменить выше.',
    filteredHint: 'Сбросьте фильтр или выберите другие даты.',
  },
} as const;

/** Loading, error and empty states of a month list (docs/design.md: designed, not blank); the list otherwise. */
export function ListStatus({
  isPending,
  error,
  isEmpty,
  isFiltered,
  scope = 'month',
  loadingLabel,
  onRetry,
  onResetFilters,
  children,
}: ListStatusProps) {
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
              Повторить
            </Button>
          }
        >
          Не удалось загрузить: {error.message}
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
          {isFiltered ? 'С этим фильтром записей нет' : emptyTexts[scope].title}
        </Typography>
        <Typography color="text.secondary" sx={{ maxWidth: 360 }}>
          {isFiltered ? emptyTexts[scope].filteredHint : emptyTexts[scope].hint}
        </Typography>
        {isFiltered && (
          <Button onClick={onResetFilters} sx={{ mt: 1 }}>
            Сбросить фильтр
          </Button>
        )}
      </Box>
    );

  return children;
}
