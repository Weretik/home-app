import { useQuery, queryOptions } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/client/api';
import { moduleKeys } from '../../../platform/client/query-client';

export type Recipient = Schema<'Recipient'>;
export type GiftPriority = Schema<'Priority'>;
export type GiftStatus = Schema<'GiftStatus'>;
/** Who gives a gift: the same three values as the recipient. */
export type Giver = Recipient;
export type GiftItem = Schema<'GiftItem'>;
export type GiftVariantItem = Schema<'GiftVariantItem'>;
export type GiftCategoryItem = Schema<'GiftCategoryItem'>;
export type GiftDetailItem = Schema<'GiftDetailItem'>;

/**
 * The query keys of the Gifts section, all under the module prefix (`moduleKeys.gifts`), which every Gifts write
 * refreshes (web ADR 0009); the home summary reads `[...moduleKeys.gifts, 'summary']` from its own feature.
 */
export const giftKeys = {
  all: moduleKeys.gifts,
  /** Both tabs. */
  items: [...moduleKeys.gifts, 'items'] as const,
  list: (archived: boolean) =>
    [...moduleKeys.gifts, 'items', { archived }] as const,
  categories: [...moduleKeys.gifts, 'categories'] as const,
  details: [...moduleKeys.gifts, 'details'] as const,
};

/** The query of `useGifts`, also used to load it ahead for offline use (web ADR 0022). */
export const giftsQuery = (archived: boolean) =>
  queryOptions({
    queryKey: giftKeys.list(archived),
    queryFn: ({ signal }) =>
      apiCall(
        api.GET('/api/gifts/items', {
          params: { query: { archived } },
          signal,
        }),
      ),
  });

/** The wanted gifts («Список», archived=false) or the gifted and dropped ones («Подаровані»), in the server's order. */
export function useGifts(archived: boolean) {
  return useQuery({ ...giftsQuery(archived) });
}

/** The query of `useGiftCategories`, also used to load it ahead for offline use (web ADR 0022). */
export const giftCategoriesQuery = () =>
  queryOptions({
    queryKey: giftKeys.categories,
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/gifts/categories', { signal })),
  });

/** The categories for the filter, the gift form and the settings. */
export function useGiftCategories() {
  return useQuery({ ...giftCategoriesQuery() });
}

/** The query of `useGiftDetails`, also used to load it ahead for offline use (web ADR 0022). */
export const giftDetailsQuery = () =>
  queryOptions({
    queryKey: giftKeys.details,
    queryFn: ({ signal }) => apiCall(api.GET('/api/gifts/details', { signal })),
  });

/** The «нюанси» of every recipient, in the server's order. */
export function useGiftDetails() {
  return useQuery({ ...giftDetailsQuery() });
}
