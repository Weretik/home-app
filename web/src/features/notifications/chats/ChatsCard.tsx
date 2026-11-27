import Box from '@mui/material/Box';
import Snackbar from '@mui/material/Snackbar';
import { useState } from 'react';
import { audienceLabels, audiences } from '../data/labels';
import { useSendTest, useUnlinkChat } from '../data/notification-writes-api';
import type { Audience, NotificationChatItem } from '../data/notifications-api';
import { InfoCard } from '../page/InfoCard';
import { ChatRow } from './ChatRow';
import { UnlinkDialog } from './UnlinkDialog';

/** «Чаты» (FR-004): Виталий, Маша and the family group in a fixed order, with «Тест» and «Отвязать». */
export function ChatsCard({ chats }: { chats: NotificationChatItem[] }) {
  const unlink = useUnlinkChat();
  const test = useSendTest();
  const [unlinking, setUnlinking] = useState<Audience | null>(null);
  const [message, setMessage] = useState<string>();
  const rows = audiences.map(
    (audience) =>
      chats.find((chat) => chat.audience === audience) ?? {
        audience,
        linked: false,
        title: null,
        linkedAtUtc: null,
      },
  );

  const closeUnlink = () => {
    unlink.reset();
    setUnlinking(null);
  };
  const sendTest = (audience: Audience) =>
    test.mutate(audience, {
      onSuccess: () => setMessage('Тест отправлен: проверьте Телеграм'),
      onError: (error) =>
        setMessage(`Не удалось отправить тест: ${error.message}`),
    });

  return (
    <InfoCard
      title="Чаты"
      caption="Куда бот пишет: каждому лично; общие сводки — в группу, если она есть"
    >
      <Box component="ul" sx={{ m: 0, p: 0, listStyle: 'none' }}>
        {rows.map((chat, index) => (
          <ChatRow
            key={chat.audience}
            chat={chat}
            divider={index > 0}
            isTesting={test.isPending && test.variables === chat.audience}
            onTest={() => sendTest(chat.audience)}
            onUnlink={() => setUnlinking(chat.audience)}
          />
        ))}
      </Box>
      {unlinking && (
        <UnlinkDialog
          name={audienceLabels[unlinking]}
          error={unlink.error?.message}
          isPending={unlink.isPending}
          onConfirm={() =>
            unlink.mutate(unlinking, {
              onSuccess: () => {
                setMessage(`Чат «${audienceLabels[unlinking]}» отвязан`);
                closeUnlink();
              },
            })
          }
          onClose={closeUnlink}
        />
      )}
      <Snackbar
        open={Boolean(message)}
        autoHideDuration={5000}
        onClose={() => setMessage(undefined)}
        message={message}
      />
    </InfoCard>
  );
}
