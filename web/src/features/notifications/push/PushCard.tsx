import Alert from '@mui/material/Alert';
import Skeleton from '@mui/material/Skeleton';
import { InfoCard } from '../page/InfoCard';
import { usePushStatus } from '../data/push-api';
import { PUSH_CAPTION, PUSH_TITLE, PushSettings } from './PushSettings';
/** «Push-сповіщення» (notifications 002): the switch for this device, a test, and the person's devices. */
export function PushCard() {
  const status = usePushStatus();
  if (status.isPending)
    return (
      <InfoCard title={PUSH_TITLE} caption={PUSH_CAPTION}>
        <Skeleton
          variant="rounded"
          height={56}
          aria-label="Завантажую стан push-сповіщень"
        />
      </InfoCard>
    );
  if (status.error)
    return (
      <InfoCard title={PUSH_TITLE} caption={PUSH_CAPTION}>
        <Alert severity="error">
          Не вдалося завантажити: {status.error.message}
        </Alert>
      </InfoCard>
    );
  return <PushSettings status={status.data} />;
}
