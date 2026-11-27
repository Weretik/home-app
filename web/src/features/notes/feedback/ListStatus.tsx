import FavoriteBorderOutlinedIcon from '@mui/icons-material/FavoriteBorderOutlined';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import Skeleton from '@mui/material/Skeleton';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';
import { cardGrid } from '../note-card/card-grid';

interface ListStatusProps {
  isPending: boolean;
  error: Error | null;
  isEmpty: boolean;
  /** A filter is on, so an empty list offers to reset it. */
  isFiltered: boolean;
  /** The empty tab's words: «Про Машу ще нічого немає». */
  emptyTitle: string;
  onRetry: () => void;
  onResetFilters: () => void;
  children: ReactNode;
}

/** Skeleton cards, the error with «Повторити», the designed empty state (FR-007); the list otherwise. */
export function ListStatus({
  isPending,
  error,
  isEmpty,
  isFiltered,
  emptyTitle,
  onRetry,
  onResetFilters,
  children,
}: ListStatusProps) {
  if (isPending)
    return (
      <Box role="progressbar" aria-label="Завантажую нотатки" sx={cardGrid}>
        {[0, 1, 2].map((card) => (
          <Card key={card} sx={{ p: 2 }}>
            <Skeleton width="30%" />
            <Skeleton width="80%" height={28} />
            <Skeleton width="60%" />
          </Card>
        ))}
      </Box>
    );

  if (error)
    return (
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
    );

  if (isEmpty)
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
          <FavoriteBorderOutlinedIcon />
        </Box>
        <Typography variant="h6" component="p">
          {isFiltered ? 'З цими фільтрами нічого немає' : emptyTitle}
        </Typography>
        <Typography color="text.secondary" sx={{ maxWidth: 360 }}>
          {isFiltered
            ? 'Скиньте фільтри, щоб побачити все.'
            : 'Додайте перше вище.'}
        </Typography>
        {isFiltered && (
          <Button onClick={onResetFilters} sx={{ mt: 1 }}>
            Скинути фільтри
          </Button>
        )}
      </Card>
    );

  return children;
}
