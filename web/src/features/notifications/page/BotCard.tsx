import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import TelegramIcon from '@mui/icons-material/Telegram';
import type { NotificationsStatus } from '../data/notifications-api';
import { formatKyivTime } from '../data/labels';
import { InfoCard } from './InfoCard';

const steps = [
  'Відкрийте бота й надішліть /start.',
  'Натисніть «Я Віталій» або «Я Маша» — цей чат стане вашим.',
  'Група не обов’язкова: без неї спільні зведення приходять кожному в особисті.',
];

/** «Бот» (FR-002, FR-003): whether the bot is set up and reachable, its link, and how to connect a chat. */
export function BotCard({ status }: { status: NotificationsStatus }) {
  return (
    <InfoCard title="Бот">
      {status.configured && status.schedulerSilent && (
        // FR-009: no scheduler call for over an hour, so nothing goes out on time.
        <Alert severity="error">
          Планувальник мовчить
          {status.lastTickAtUtc
            ? ` з ${formatKyivTime(status.lastTickAtUtc)}`
            : ': жодного виклику ще не було'}
          , тож зведення не приходять. Перевірте cron-job.org (docs/deploy.md).
        </Alert>
      )}
      {!status.configured ? (
        <Alert severity="warning">
          Бот ще не налаштовано: додайте токен від @BotFather у налаштування{' '}
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
            Бот працює: <strong>@{status.botUsername}</strong>
          </Typography>
          {status.botLink && (
            <Button
              variant="contained"
              startIcon={<TelegramIcon />}
              href={status.botLink}
              target="_blank"
              rel="noopener noreferrer"
            >
              Відкрити бота
            </Button>
          )}
        </Box>
      ) : (
        <Alert severity="info">
          Токен є, але Телеграм зараз не відповів. Повідомлення підуть, коли він
          буде доступний.
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
