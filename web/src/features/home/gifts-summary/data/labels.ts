import type { Recipient } from './summary-api';

/** Own copy of the Gifts labels: features never import each other (web ADR 0008). */
export const recipientLabels: Record<Recipient, string> = {
  Maria: 'Маша',
  Vitaliy: 'Виталий',
  Together: 'Вместе',
};

export const recipientEmoji: Record<Recipient, string> = {
  Maria: '👩',
  Vitaliy: '👨',
  Together: '🏠',
};

export const recipientOrder: readonly Recipient[] = [
  'Maria',
  'Vitaliy',
  'Together',
];
