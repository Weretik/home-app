import PeopleOutlinedIcon from '@mui/icons-material/PeopleOutlined';
import Avatar from '@mui/material/Avatar';
import { assigneeInitials, assigneeLabels } from '../data/labels';
import type { Assignee } from '../data/todos-api';

/** Whose task it is: «М», «В», or two people for a shared one; the full name is its label and tooltip. */
export function AssigneeAvatar({ assignee }: { assignee: Assignee }) {
  const label = `Кому: ${assigneeLabels[assignee]}`;
  return (
    <Avatar
      role="img"
      aria-label={label}
      title={label}
      sx={(theme) => ({
        width: 22,
        height: 22,
        fontSize: 12,
        fontWeight: 700,
        color: assignee === 'Maria' ? 'secondary.main' : 'primary.main',
        bgcolor:
          assignee === 'Maria'
            ? `rgba(${theme.vars.palette.secondary.mainChannel} / 0.12)`
            : `rgba(${theme.vars.palette.primary.mainChannel} / 0.12)`,
      })}
    >
      {assignee === 'Shared' ? (
        <PeopleOutlinedIcon sx={{ fontSize: 14 }} />
      ) : (
        assigneeInitials[assignee]
      )}
    </Avatar>
  );
}
