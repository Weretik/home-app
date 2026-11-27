import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import FormControlLabel from '@mui/material/FormControlLabel';
import Snackbar from '@mui/material/Snackbar';
import Switch from '@mui/material/Switch';
import { useState } from 'react';
import { InfoCard } from '../page/InfoCard';
import { useDeletePushDevice, useSendPushTest } from '../data/push-api';
import { DeviceRow } from './DeviceRow';
import { pushBlocker } from './push-support';
import { PermissionDeniedError, usePushDevice } from './use-push-device';
import type { PushStatus } from '../data/push-api';

export const PUSH_TITLE = 'Push-сповіщення';
export const PUSH_CAPTION =
  'Ті самі повідомлення, що й у Telegram, на телефоні чи комп’ютері';

/** The loaded card: the switch for this device, a test, and the person's devices (FR-102…FR-106). */
export function PushSettings({ status }: { status: PushStatus }) {
  const device = usePushDevice(status.devices);
  const test = useSendPushTest();
  const remove = useDeletePushDevice();
  const [notice, setNotice] = useState<string | null>(null);

  const blocker = pushBlocker(status, device.support);

  const sendTest = () =>
    test.mutate(undefined, {
      onSuccess: (result) =>
        setNotice(`Надіслано на пристрої: ${result.delivered}`),
      onError: (error) => setNotice(error.message),
    });

  return (
    <InfoCard title={PUSH_TITLE} caption={PUSH_CAPTION}>
      {blocker ? (
        <Alert severity="info">{blocker}</Alert>
      ) : (
        <Box
          sx={{
            display: 'flex',
            flexWrap: 'wrap',
            alignItems: 'center',
            gap: 1,
          }}
        >
          <FormControlLabel
            sx={{ mr: 'auto' }}
            label="Сповіщення на цьому пристрої"
            control={
              <Switch
                checked={device.on}
                disabled={device.pending}
                onChange={(_, on) =>
                  on && status.publicKey
                    ? device.enable(status.publicKey)
                    : device.disable()
                }
              />
            }
          />
          {status.devices.length > 0 && (
            <Button
              variant="outlined"
              size="small"
              disabled={test.isPending}
              onClick={sendTest}
            >
              Надіслати тест
            </Button>
          )}
        </Box>
      )}
      {!blocker &&
        (device.denied || device.error instanceof PermissionDeniedError) && (
          <Alert severity="warning">
            Сповіщення заборонені в браузері. Дозвольте їх у налаштуваннях сайту
            (значок біля адреси) і ввімкніть знову.
          </Alert>
        )}
      {device.error && !(device.error instanceof PermissionDeniedError) && (
        <Alert severity="error">
          Не вдалося змінити: {device.error.message}
        </Alert>
      )}
      {status.devices.length > 0 && (
        <Box
          component="ul"
          aria-label="Мої пристрої"
          sx={{ m: 0, p: 0, listStyle: 'none' }}
        >
          {status.devices.map((item, index) => (
            <DeviceRow
              key={item.id}
              device={item}
              first={index === 0}
              current={item.id === device.thisDevice?.id}
              removing={remove.isPending}
              onRemove={() =>
                remove.mutate(item.id, {
                  onError: (error) => setNotice(error.message),
                })
              }
            />
          ))}
        </Box>
      )}
      <Snackbar
        open={notice !== null}
        autoHideDuration={5000}
        onClose={() => setNotice(null)}
        message={notice}
      />
    </InfoCard>
  );
}
