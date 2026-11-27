import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';
import { RetryButton } from './RetryButton';
import { OfflineNotice } from '../../../platform/offline/status/OfflineNotice';
import { useOnline } from '../../../platform/offline/status/use-network-status';

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
  const online = useOnline();
  if (isPending && !online) return <OfflineNotice />;
  if (isPending)
    return (
      <Box sx={{ display: 'grid', placeItems: 'center', py: 6 }}>
        <CircularProgress aria-label={loadingLabel} />
      </Box>
    );

  if (error && !hasData)
    return (
      <Alert severity="error" action={<RetryButton onRetry={onRetry} />}>
        Не вдалося завантажити: {error.message}
      </Alert>
    );

  return (
    <>
      {error && (
        <Alert severity="warning" action={<RetryButton onRetry={onRetry} />}>
          Немає зв’язку з сервером — показано останнє, що вдалося завантажити.
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
