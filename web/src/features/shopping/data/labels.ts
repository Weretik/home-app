import { locale } from '../../../platform/locale';
import type { ShoppingItem, ShoppingPerson } from './shopping-api';

export const personLabels: Record<ShoppingPerson, string> = {
  Vitaliy: 'Віталій',
  Maria: 'Маша',
};

const time = new Intl.DateTimeFormat(locale, {
  hour: '2-digit',
  minute: '2-digit',
});
const dayMonth = new Intl.DateTimeFormat(locale, {
  day: 'numeric',
  month: 'short',
});

/** «14:05» today, «3 жовт., 14:05» on another day. */
export function formatWhen(iso: string, now: Date = new Date()): string {
  const date = new Date(iso);
  const sameDay = date.toDateString() === now.toDateString();
  return sameDay
    ? time.format(date)
    : `${dayMonth.format(date)}, ${time.format(date)}`;
}

/** The row in one line for a confirmation: «Молоко · 2 л». */
export const itemTitle = (item: Pick<ShoppingItem, 'name' | 'quantity'>) =>
  item.quantity ? `${item.name} · ${item.quantity}` : item.name;

/** «Віталій» or «Маша»; unknown values are shown as they are. */
export const personLabel = (person: string | null | undefined) =>
  person ? (personLabels[person as ShoppingPerson] ?? person) : '';
