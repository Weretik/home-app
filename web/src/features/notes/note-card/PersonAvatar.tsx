import Avatar from '@mui/material/Avatar';
import { personAbout, personInitials } from '../data/labels';
import type { NotePerson } from '../data/notes-api';

/** The person colours of docs/design.md: Віталій `primary`, Маша `secondary`. */
const tones = { Vitaliy: 'primary', Maria: 'secondary' } as const;

/** Whom a note is about, as a «В» or «М» circle in the person's colour, shown in «Усі» (FR-004). */
export function PersonAvatar({ person }: { person: NotePerson }) {
  const tone = tones[person];
  return (
    <Avatar
      component="span"
      role="img"
      aria-label={`Про ${personAbout[person]}`}
      sx={{
        width: 24,
        height: 24,
        fontSize: 13,
        fontWeight: 700,
        bgcolor: `${tone}.main`,
        color: `${tone}.contrastText`,
      }}
    >
      {personInitials[person]}
    </Avatar>
  );
}
