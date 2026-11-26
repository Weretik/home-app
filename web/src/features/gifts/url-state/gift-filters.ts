import type { GiftPriority, Recipient } from '../data/gifts-api';
import { priorities, recipients } from '../display/labels';

/** The list filters a link carries (`?recipient=&category=&priority=`); only known values. */
export interface GiftFilters {
  recipient?: Recipient;
  categoryId?: string;
  priority?: GiftPriority;
}

interface Filterable {
  recipient: Recipient;
  categoryId: string;
  priority: GiftPriority;
}

interface Priced {
  price?: number | null;
}

export interface GiftGroup<T> {
  recipient: Recipient;
  items: T[];
  /** The sum of the known prices in the group. */
  priceSum: number;
}

const isRecipient = (value: string | null): value is Recipient =>
  (recipients as readonly (string | null)[]).includes(value);

const isPriority = (value: string | null): value is GiftPriority =>
  (priorities as readonly (string | null)[]).includes(value);

/**
 * Reads the filters from the URL. An unknown recipient or priority is ignored; a category is kept only when it exists, or while
 * the categories are not loaded yet (`categoryIds` undefined).
 */
export function parseFilters(
  params: URLSearchParams,
  categoryIds?: readonly string[],
): GiftFilters {
  const filters: GiftFilters = {};
  const recipient = params.get('recipient');
  if (isRecipient(recipient)) filters.recipient = recipient;
  const category = params.get('category');
  if (category && (!categoryIds || categoryIds.includes(category)))
    filters.categoryId = category;
  const priority = params.get('priority');
  if (isPriority(priority)) filters.priority = priority;
  return filters;
}

/** The items that match the filters, in the server's order. */
export function applyFilters<T extends Filterable>(
  items: readonly T[],
  filters: GiftFilters,
): T[] {
  return items.filter(
    (item) =>
      (!filters.recipient || item.recipient === filters.recipient) &&
      (!filters.categoryId || item.categoryId === filters.categoryId) &&
      (!filters.priority || item.priority === filters.priority),
  );
}

/** The items per recipient (Маша, Виталий, Вместе) with their price sum, server order kept; empty groups left out. */
export function groupByRecipient<T extends Filterable & Priced>(
  items: readonly T[],
): GiftGroup<T>[] {
  return recipients
    .map((recipient) => {
      const own = items.filter((item) => item.recipient === recipient);
      const priceSum = own.reduce((sum, item) => sum + (item.price ?? 0), 0);
      return { recipient, items: own, priceSum };
    })
    .filter((group) => group.items.length > 0);
}
