import { useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/api';

export type Recipient = Schema<'Recipient'>;
export type GiftPriority = Schema<'Priority'>;
export type GiftStatus = Schema<'GiftStatus'>;
export type GiftItem = Schema<'GiftItem'>;
export type GiftVariantItem = Schema<'GiftVariantItem'>;
export type GiftCategoryItem = Schema<'GiftCategoryItem'>;
export type GiftDetailItem = Schema<'GiftDetailItem'>;

/** The query keys of the Gifts section; the home summary reads `['gifts', 'summary']` from its own feature. */
export const giftKeys = {
  /** Both tabs; a gift write refreshes them together. */
  items: ['gifts', 'items'] as const,
  list: (archived: boolean) => ['gifts', 'items', { archived }] as const,
  categories: ['gifts', 'categories'] as const,
  details: ['gifts', 'details'] as const,
  summary: ['gifts', 'summary'] as const,
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
