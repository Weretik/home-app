import CssBaseline from '@mui/material/CssBaseline';
import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { PersistQueryClientProvider } from '@tanstack/react-query-persist-client';
import { useEffect, useState, type ReactNode } from 'react';
import { createQueryClient } from '../../platform/client/query-client';
import {
  cachePersistence,
  indexedDbStorage,
} from '../../platform/offline/query-persistence';
import { registerQueuedWrites } from '../../platform/offline/queued-write-api';
import { session } from '../../platform/session/session';
import { theme } from '../theme/theme';

function createClient() {
  const queryClient = createQueryClient();
  registerQueuedWrites(queryClient);
  return queryClient;
}

/**
 * React Query with its cache kept in IndexedDB (web ADR 0020): the data last seen shows offline, and the queued writes
 * restored after a restart are sent again. A new session forgets the kept cache, like the in-memory one (ADR 0015).
 */
export function Providers({ children }: { children: ReactNode }) {
  const [queryClient] = useState(createClient);
  const [persistence] = useState(() =>
    cachePersistence(indexedDbStorage(), __API_CONTRACT_HASH__),
  );

  useEffect(
    () => session.subscribe(() => void persistence?.persister.removeClient()),
    [persistence],
  );

  const content = (
    <ThemeProvider theme={theme} defaultMode="system">
      <CssBaseline />
      {children}
    </ThemeProvider>
  );

  return persistence ? (
    <PersistQueryClientProvider
      client={queryClient}
      persistOptions={persistence}
      onSuccess={() => void queryClient.resumePausedMutations()}
    >
      {content}
    </PersistQueryClientProvider>
  ) : (
    <QueryClientProvider client={queryClient}>{content}</QueryClientProvider>
  );
}
