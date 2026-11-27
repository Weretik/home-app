import GroupOutlinedIcon from '@mui/icons-material/GroupOutlined';
import Avatar from '@mui/material/Avatar';
import type { Recipient } from '../data/gifts-api';
import { recipientTones } from './recipient-tones';

const initials: Record<Recipient, string | undefined> = {
  Maria: 'М',
  Vitaliy: 'В',
  Together: undefined,
};

/** The person's circle in their colour: an initial, or two people for «Разом». Decorative; the name is next to it. */
export function RecipientAvatar({
  recipient,
  size = 32,
}: {
  recipient: Recipient;
  size?: number;
}) {
  const tone = recipientTones[recipient];
  return (
    <Avatar
      aria-hidden
      sx={{
        width: size,
        height: size,
        fontSize: size * 0.45,
        fontWeight: 700,
        bgcolor: `${tone}.main`,
        color: `${tone}.contrastText`,
      }}
    >
      {initials[recipient] ?? (
        <GroupOutlinedIcon sx={{ fontSize: size * 0.6 }} />
      )}
    </Avatar>
  );
}
