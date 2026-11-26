import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import { audienceLabels, formatKyivTime } from '../data/labels';
import type { NotificationChatItem } from '../data/notifications-api';

interface ChatRowProps {
  chat: NotificationChatItem;
  divider: boolean;
  isTesting: boolean;
  onTest: () => void;
  onUnlink: () => void;
}

/** One chat (FR-004): whose it is, its Telegram title and since when, or «не подключён»; «Тест» and «Отвязать». */
export function ChatRow({
  chat,
  divider,
  isTesting,
  onTest,
  onUnlink,
}: ChatRowProps) {
  const name = audienceLabels[chat.audience];
  return (
    <Box
      component="li"
      sx={{
        py: 1.25,
        borderTop: divider ? 1 : 0,
        borderColor: 'divider',
        display: 'flex',
        flexWrap: 'wrap',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: 1,
      }}
    >
      <Box sx={{ minWidth: 0 }}>
        <Typography sx={{ fontWeight: 500 }}>{name}</Typography>
        <Typography
          variant="body2"
          color="text.secondary"
          sx={{ overflowWrap: 'anywhere' }}
        >
          {chat.linked
            ? [
                chat.title,
                chat.linkedAtUtc &&
                  `с ${formatKyivTime(chat.linkedAtUtc).split(',')[0]}`,
              ]
                .filter(Boolean)
                .join(' · ') || 'подключён'
            : 'не подключён'}
        </Typography>
      </Box>
      {chat.linked && (
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            size="small"
            onClick={onTest}
            disabled={isTesting}
            aria-label={`Тест: ${name}`}
          >
            {isTesting ? 'Отправляю…' : 'Тест'}
          </Button>
          <Button
            size="small"
            color="error"
            onClick={onUnlink}
            aria-label={`Отвязать: ${name}`}
          >
            Отвязать
          </Button>
        </Box>
      )}
    </Box>
  );
}
