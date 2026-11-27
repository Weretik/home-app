import type { GiftPriority, GiftStatus, Recipient } from '../data/gifts-api';

/** The recipients in the order the lists and groups show them. */
export const recipients: readonly Recipient[] = [
  'Maria',
  'Vitaliy',
  'Together',
];

export const recipientLabels: Record<Recipient, string> = {
  Maria: 'Маша',
  Vitaliy: 'Віталій',
  Together: 'Разом',
};

/** The priorities from the most wanted, as the form offers them. */
export const priorities: readonly GiftPriority[] = [
  'High',
  'Normal',
  'Someday',
];

export const priorityLabels: Record<GiftPriority, string> = {
  High: '🔥 Дуже хочу',
  Normal: '⭐ Хочу',
  Someday: '💭 Колись',
};

export const statusLabels: Record<GiftStatus, string> = {
  Wanted: 'Чекає',
  Gifted: 'Подаровано',
  Dropped: 'Неактуально',
};
