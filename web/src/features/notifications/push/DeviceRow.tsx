import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import { formatKyivTime } from '../data/labels';
import type { PushDevice } from '../data/push-api';

interface DeviceRowProps {
  device: PushDevice;
  current: boolean;
  first: boolean;
  removing: boolean;
  onRemove: () => void;
}

/** One of the person's devices (FR-106); this device is switched off with the switch, not removed here. */
export function DeviceRow({
  device,
  current,
  first,
  removing,
  onRemove,
}: DeviceRowProps) {
  return (
    <Box
      component="li"
      sx={{
        py: 1,
        borderTop: first ? 0 : 1,
        borderColor: 'divider',
        display: 'flex',
        flexWrap: 'wrap',
        alignItems: 'center',
        justifyContent: 'space-between',
        columnGap: 2,
      }}
    >
      <Box sx={{ minWidth: 0 }}>
        <Typography sx={{ fontWeight: 500 }}>
          {device.device ?? 'Пристрій'}
          {current && (
            <Typography component="span" color="primary.main" sx={{ ml: 1 }}>
              цей пристрій
            </Typography>
          )}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          востаннє {formatKyivTime(device.lastSeenAtUtc)}
        </Typography>
      </Box>
      {!current && (
        <Button
          size="small"
          color="error"
          disabled={removing}
          onClick={onRemove}
        >
          Видалити
        </Button>
      )}
    </Box>
  );
}
