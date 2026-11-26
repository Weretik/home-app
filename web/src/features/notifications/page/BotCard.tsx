import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import TelegramIcon from '@mui/icons-material/Telegram';
import type { NotificationsStatus } from '../data/notifications-api';
import { InfoCard } from './InfoCard';

const steps = [
  'Откройте бота и отправьте /start.',
  'Нажмите «Я Виталий» или «Я Маша» — этот чат будет вашим.',
  'Добавьте бота в семейную группу — он подключит её сам.',
];

/** «Бот» (FR-002, FR-003): whether the bot is set up and reachable, its link, and how to connect a chat. */
export function BotCard({ status }: { status: NotificationsStatus }) {
  return (
    <InfoCard title="Бот">
      {!status.configured ? (
        <Alert severity="warning">
          Бот ещё не настроен: добавьте токен от @BotFather в настройку{' '}
          <code>Telegram__BotToken</code> сервера (App Service).
        </Alert>
      ) : status.botUsername ? (
        <Box
          sx={{
            display: 'flex',
            flexWrap: 'wrap',
            alignItems: 'center',
            justifyContent: 'space-between',
            gap: 1.5,
          }}
        >
          <Typography>
            Бот работает: <strong>@{status.botUsername}</strong>
          </Typography>
          {status.botLink && (
            <Button
              variant="contained"
              startIcon={<TelegramIcon />}
              href={status.botLink}
              target="_blank"
              rel="noopener noreferrer"
            >
              Открыть бота
            </Button>
          )}
        </Box>
      ) : (
        <Alert severity="info">
          Токен есть, но Телеграм сейчас не ответил. Сообщения уйдут, когда он
          будет доступен.
        </Alert>
      )}
      {status.configured && (
        <Box component="ol" sx={{ m: 0, pl: 2.5, display: 'grid', gap: 0.5 }}>
          {steps.map((step) => (
            <Typography key={step} component="li" variant="body2">
              {step}
            </Typography>
          ))}
        </Box>
      )}
    </InfoCard>
  );
}
