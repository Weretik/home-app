import type { SummaryItem } from './shopping-summary-api';

/** The summary's own copy of the person names (features never import each other, web ADR 0008). */
const people: Record<string, string> = { Vitaliy: 'Віталій', Maria: 'Маша' };

export const personName = (person: string) => people[person] ?? person;

/** «Молоко · 2 л». */
export const itemLine = (item: Pick<SummaryItem, 'name' | 'quantity'>) =>
  item.quantity ? `${item.name} · ${item.quantity}` : item.name;

/** «1 раз», «3 рази», «7 разів». */
export function times(n: number): string {
  const tens = n % 100;
  const ones = n % 10;
  if (tens >= 11 && tens <= 14) return `${n} разів`;
  if (ones === 1) return `${n} раз`;
  if (ones >= 2 && ones <= 4) return `${n} рази`;
  return `${n} разів`;
}
