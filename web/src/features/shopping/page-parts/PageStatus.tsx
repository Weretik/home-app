import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';

interface PageStatusProps {
  isPending: boolean;
  error: Error | null;
  /** True when data is shown despite the error (a failed reload): the page stays with a warning. */
  hasData: boolean;
  isEmpty: boolean;
  loadingLabel: string;
  empty: { emoji: string; title: string; hint: string };
  onRetry: () => void;
  children: ReactNode;
}

const RetryButton = ({ onRetry }: { onRetry: () => void }) => (
  <Button color="inherit" size="small" onClick={onRetry}>
    Повторить
  </Button>
);

/** Loading, error, offline and empty states of a page (docs/design.md: designed, not blank); the content otherwise. */
export function PageStatus({
  isPending,
  error,
  hasData,
  isEmpty,
  loadingLabel,
  empty,
  onRetry,
  children,
}: PageStatusProps) {
  if (isPending)
    return (
      <Box sx={{ display: 'grid', placeItems: 'center', py: 6 }}>
        <CircularProgress aria-label={loadingLabel} />
      </Box>
    );

  if (error && !hasData)
    return (
      <Alert severity="error" action={<RetryButton onRetry={onRetry} />}>
        Не удалось загрузить: {error.message}
      </Alert>
    );

  return (
    <>
      {error && (
        <Alert severity="warning" action={<RetryButton onRetry={onRetry} />}>
          Нет связи с сервером — показано последнее, что удалось загрузить.
        </Alert>
      )}
      {isEmpty ? (
        <Box
          sx={{
            p: { xs: 3, md: 5 },
            display: 'grid',
            justifyItems: 'center',
            textAlign: 'center',
            gap: 1,
          }}
        >
          <Typography aria-hidden sx={{ fontSize: 40, lineHeight: 1 }}>
            {empty.emoji}
          </Typography>
          <Typography variant="h6" component="p">
            {empty.title}
          </Typography>
          <Typography color="text.secondary" sx={{ maxWidth: 360 }}>
            {empty.hint}
          </Typography>
        </Box>
      ) : (
        children
      )}
    </>
  );
}
