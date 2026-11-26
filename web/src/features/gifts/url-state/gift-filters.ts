import type { Recipient } from '../data/gifts-api';
import { recipients } from '../display/labels';

/** The list filters a link carries (`?recipient=&category=`); only known values. */
export interface GiftFilters {
  recipient?: Recipient;
  categoryId?: string;
}

interface Filterable {
  recipient: Recipient;
  categoryId: string;
}

export interface GiftGroup<T> {
  recipient: Recipient;
  items: T[];
}

const isRecipient = (value: string | null): value is Recipient =>
  (recipients as readonly (string | null)[]).includes(value);

/**
 * Reads the filters from the URL. An unknown recipient is ignored; a category is kept only when it exists, or while
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
      (!filters.categoryId || item.categoryId === filters.categoryId),
  );
}

/** The items per recipient (Маша, Виталий, Вместе), server order kept inside a group; empty groups left out. */
export function groupByRecipient<T extends Filterable>(
  items: readonly T[],
): GiftGroup<T>[] {
  return recipients
    .map((recipient) => ({
      recipient,
      items: items.filter((item) => item.recipient === recipient),
    }))
    .filter((group) => group.items.length > 0);
}
