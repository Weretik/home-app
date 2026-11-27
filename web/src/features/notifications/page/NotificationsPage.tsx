import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Skeleton from '@mui/material/Skeleton';
import Typography from '@mui/material/Typography';
import { ChatsCard } from '../chats/ChatsCard';
import { useNotificationsStatus } from '../data/notifications-api';
import { BotCard } from './BotCard';
import { DeliveriesCard } from './DeliveriesCard';
import { ScheduleCard } from './ScheduleCard';

/** «Телеграм» (notifications 001): the bot, the linked chats, the schedule and the last deliveries. */
export function NotificationsPage() {
  const status = useNotificationsStatus();

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <Box>
        <Typography variant="h4" component="h1">
          Телеграм
        </Typography>
        <Typography color="text.secondary">
          Бот надсилає справи, покупки, зведення витрат і нагадування про
          подарунки
        </Typography>
      </Box>
      {status.isPending ? (
        <Box aria-label="Завантажую стан бота" sx={{ display: 'grid', gap: 2 }}>
          <Skeleton variant="rounded" height={120} />
          <Skeleton variant="rounded" height={180} />
        </Box>
      ) : status.error ? (
        <Alert
          severity="error"
          action={
            <Button
              color="inherit"
              size="small"
              onClick={() => void status.refetch()}
            >
              Повторити
            </Button>
          }
        >
          Не вдалося завантажити: {status.error.message}
        </Alert>
      ) : (
        <Box
          sx={{
            display: 'grid',
            gap: { xs: 2, md: 3 },
            gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' },
            alignItems: 'start',
          }}
        >
          <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
            <BotCard status={status.data} />
            <ChatsCard chats={status.data.chats} />
          </Box>
          <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
            <ScheduleCard schedule={status.data.schedule} />
            <DeliveriesCard status={status.data} />
          </Box>
        </Box>
      )}
    </Box>
  );
}
