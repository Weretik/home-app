import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Skeleton from '@mui/material/Skeleton';
import Typography from '@mui/material/Typography';
import { Link as RouterLink } from 'react-router-dom';
import type { FamilyPerson } from '../../../platform/member-api';
import { useTelegramChat } from './profile-api';

/** «Телеграм»: whether the member's private chat with the bot is linked, with a way to the «Телеграм» page. */
export function TelegramRow({ person }: { person: FamilyPerson | undefined }) {
  const chat = useTelegramChat(person);

  const state = !person
    ? 'Бот пише лише Віталію та Маші'
    : chat.isPending
      ? null
      : chat.error
        ? 'Не вдалося дізнатися'
        : chat.data?.linked
          ? `Підключено${chat.data.title ? `: ${chat.data.title}` : ''}`
          : 'Не підключено';

  return (
    <Box
      sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}
    >
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <Typography variant="body2" sx={{ color: 'text.secondary' }}>
          Телеграм
        </Typography>
        {state === null ? (
          <Skeleton width={160} aria-label="Завантажую Телеграм" />
        ) : (
          <Typography>{state}</Typography>
        )}
      </Box>
      <Button component={RouterLink} to="/notifications" size="small">
        {chat.data?.linked ? 'Налаштування' : 'Підключити'}
      </Button>
    </Box>
  );
}
