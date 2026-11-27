import Avatar from '@mui/material/Avatar';
import type { FamilyMember } from '../data/identity-api';

interface MemberAvatarProps {
  /** The member, or undefined while it loads: a neutral placeholder then (FR-007). */
  member: FamilyMember | undefined;
  size?: number;
}

/** The member's Google picture, else the first letter of the name. */
export function MemberAvatar({ member, size = 32 }: MemberAvatarProps) {
  const sx = { width: size, height: size, fontSize: size * 0.45 };
  if (!member) return <Avatar sx={sx} />;
  return (
    <Avatar
      src={member.pictureUrl ?? undefined}
      alt={member.name}
      // Google serves the picture only without a referrer from another site.
      slotProps={{ img: { referrerPolicy: 'no-referrer' } }}
      sx={{ ...sx, bgcolor: 'primary.main', color: 'primary.contrastText' }}
    >
      {member.name.trim().charAt(0).toUpperCase() || '?'}
    </Avatar>
  );
}
