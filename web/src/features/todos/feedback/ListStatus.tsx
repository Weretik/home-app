import ChecklistOutlinedIcon from '@mui/icons-material/ChecklistOutlined';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Skeleton from '@mui/material/Skeleton';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';

interface ListStatusProps {
  isPending: boolean;
  error: Error | null;
  isEmpty: boolean;
  /** A filter is on, so an empty list offers to reset it. */
  isFiltered: boolean;
  /** The empty list's words: what will appear here and how. */
  empty: { title: string; hint: string };
  loadingLabel: string;
  onRetry: () => void;
  onResetFilters: () => void;
  children: ReactNode;
}

/** Loading rows, the error with «Повторити», the designed empty state (FR-009); the list otherwise. */
export function ListStatus({
  isPending,
  error,
  isEmpty,
  isFiltered,
  empty,
  loadingLabel,
  onRetry,
  onResetFilters,
  children,
}: ListStatusProps) {
  if (isPending)
    return (
      <Box
        role="progressbar"
        aria-label={loadingLabel}
        sx={{ p: 2, display: 'grid', gap: 1.5 }}
      >
        {[0, 1, 2].map((row) => (
          <Box key={row} sx={{ display: 'flex', gap: 1.5 }}>
            <Skeleton variant="circular" width={24} height={24} />
            <Box sx={{ flex: 1 }}>
              <Skeleton width="70%" />
              <Skeleton width="40%" />
            </Box>
          </Box>
        ))}
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
          <ChecklistOutlinedIcon />
        </Box>
        <Typography variant="h6" component="p">
          {isFiltered ? 'З цим фільтром справ немає' : empty.title}
        </Typography>
        <Typography color="text.secondary" sx={{ maxWidth: 360 }}>
          {isFiltered ? 'Скиньте фільтр, щоб побачити всі.' : empty.hint}
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
