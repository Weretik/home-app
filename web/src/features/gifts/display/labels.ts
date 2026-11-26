import type { GiftPriority, GiftStatus, Recipient } from '../data/gifts-api';

/** The recipients in the order the lists and groups show them. */
export const recipients: readonly Recipient[] = [
  'Maria',
  'Vitaliy',
  'Together',
];

export const recipientLabels: Record<Recipient, string> = {
  Maria: 'Маша',
  Vitaliy: 'Виталий',
  Together: 'Вместе',
};

/** The priorities from the most wanted, as the form offers them. */
export const priorities: readonly GiftPriority[] = [
  'High',
  'Normal',
  'Someday',
];

export const priorityLabels: Record<GiftPriority, string> = {
  High: '🔥 Очень хочу',
  Normal: '⭐ Хочу',
  Someday: '💭 Когда-нибудь',
};

export const statusLabels: Record<GiftStatus, string> = {
  Wanted: 'Ждёт',
  Gifted: 'Подарено',
  Dropped: 'Не актуально',
};
