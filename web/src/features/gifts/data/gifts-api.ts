import { useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/api';
import { moduleKeys } from '../../../platform/query-keys';

export type Recipient = Schema<'Recipient'>;
export type GiftPriority = Schema<'Priority'>;
export type GiftStatus = Schema<'GiftStatus'>;
/** Who gives a gift: the same three values as the recipient. */
export type Giver = Recipient;
// TEMP shim (notifications 001): `giver` and `buyBy` until the client is regenerated from the new contract.
export type GiftItem = Schema<'GiftItem'> & {
  giver?: Giver | null;
  buyBy?: string | null;
};
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

/** The wanted gifts («Список», archived=false) or the gifted and dropped ones («Подаренные»), in the server's order. */
export function useGifts(archived: boolean) {
  return useQuery({
    queryKey: giftKeys.list(archived),
    queryFn: ({ signal }) =>
      apiCall(
        api.GET('/api/gifts/items', {
          params: { query: { archived } },
          signal,
        }),
      ),
  });
}

/** The categories for the filter, the gift form and the settings. */
export function useGiftCategories() {
  return useQuery({
    queryKey: giftKeys.categories,
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/gifts/categories', { signal })),
  });
}

/** The «нюансы» of every recipient, in the server's order. */
export function useGiftDetails() {
  return useQuery({
    queryKey: giftKeys.details,
    queryFn: ({ signal }) => apiCall(api.GET('/api/gifts/details', { signal })),
  });
}
