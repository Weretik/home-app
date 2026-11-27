import CloudOffOutlinedIcon from '@mui/icons-material/CloudOffOutlined';
import CloudSyncOutlinedIcon from '@mui/icons-material/CloudSyncOutlined';
import Alert from '@mui/material/Alert';
import Chip from '@mui/material/Chip';
import Snackbar from '@mui/material/Snackbar';
import { refusedWrites } from '../../platform/offline/status/network-status';
import {
  useOnline,
  useQueuedWriteCount,
  useRefusedWrites,
} from '../../platform/offline/status/use-network-status';

/** One chip for both states, short enough for a 360 px header; the full words go to `aria-label`. */
const chipLabel = (online: boolean, waiting: number) => {
  if (online) return `Очікує: ${waiting}`;
  return waiting > 0 ? `Офлайн · ${waiting}` : 'Офлайн';
};
const chipName = (online: boolean, waiting: number) =>
  !online && waiting > 0 ? `Офлайн, очікує: ${waiting}` : undefined;

/**
 * The top bar's network state (FR-005): «Офлайн» without a network, «Очікує: N» while queued writes wait, nothing when
 * all is sent; and «Не вдалося зберегти: …» when the server refused a queued write.
 */
export function OfflineStatus() {
  const online = useOnline();
  const waiting = useQueuedWriteCount();
  const refused = useRefusedWrites()[0];

  return (
    <>
      {(!online || waiting > 0) && (
        <Chip
          size="small"
          variant="outlined"
          color={online ? 'default' : 'warning'}
          icon={online ? <CloudSyncOutlinedIcon /> : <CloudOffOutlinedIcon />}
          label={chipLabel(online, waiting)}
          aria-label={chipName(online, waiting)}
          role="status"
          sx={{
            flex: 'none',
            fontVariantNumeric: 'tabular-nums',
            // At 360 px the header has room for the words only.
            '& .MuiChip-icon': { display: { xs: 'none', sm: 'flex' } },
          }}
        />
      )}
      <Snackbar
        key={refused?.id}
        open={refused !== undefined}
        autoHideDuration={8000}
        onClose={(_, reason) =>
          reason !== 'clickaway' && refused && refusedWrites.dismiss(refused.id)
        }
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
        sx={{
          bottom: {
            xs: 'calc(88px + env(safe-area-inset-bottom, 0px))',
            md: 24,
          },
        }}
      >
        <Alert
          severity="error"
          variant="filled"
          closeText="Закрити"
          onClose={() => refused && refusedWrites.dismiss(refused.id)}
        >
          Не вдалося зберегти: {refused?.label}
        </Alert>
      </Snackbar>
    </>
  );
}
