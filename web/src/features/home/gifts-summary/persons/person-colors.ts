import type { Theme } from '@mui/material/styles';
import type { Recipient } from '../data/summary-api';

export type PersonColor = 'primary' | 'secondary' | 'info';

/**
 * One colour per person in the Gifts summary, the same as on the Gifts screens (docs/design.md): Віталій primary,
 * Маша secondary, Разом info. Own copy of the mapping, since features never import each other (web ADR 0008).
 */
export const personColors: Record<Recipient, PersonColor> = {
  Vitaliy: 'primary',
  Maria: 'secondary',
  Together: 'info',
};

/** The person's tint behind their block: the main colour at 0.08, following the colour scheme. */
export const personTint = (theme: Theme, recipient: Recipient, alpha = 0.08) =>
  `rgba(${theme.vars.palette[personColors[recipient]].mainChannel} / ${alpha})`;
