import type { Theme } from '@mui/material/styles';
import type { Recipient } from '../data/gifts-api';

type Tone = 'primary' | 'secondary' | 'info';

/**
 * One colour per person on every Gifts screen (docs/design.md, «Person colours»): Виталий primary, Маша secondary,
 * Вместе info. The home summary keeps its own copy of this mapping, since features never import each other.
 */
export const recipientTones: Record<Recipient, Tone> = {
  Vitaliy: 'primary',
  Maria: 'secondary',
  Together: 'info',
};

/** The person's colour: the avatar, a filter's mark, the group's accent line. */
export const toneColor = (theme: Theme, recipient: Recipient) =>
  theme.vars.palette[recipientTones[recipient]].main;

/** The person's tint: 0.08 behind a group header, 0.12 for a picked filter. */
export const toneTint = (theme: Theme, recipient: Recipient, alpha = 0.08) =>
  `rgba(${theme.vars.palette[recipientTones[recipient]].mainChannel} / ${alpha})`;
