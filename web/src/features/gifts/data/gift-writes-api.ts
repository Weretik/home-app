import { useMutation } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/client/api';
import { moduleKeys } from '../../../platform/client/query-client';
import { giftKeys, type GiftItem, type GiftStatus } from './gifts-api';
import { isStale } from './write-errors-api';
import {
  ifMatch,
  useRefresh,
  useRemove,
  useSave,
  type Writer,
} from './writes-api';

export type GiftRequest = Schema<'GiftRequest'>;
export type GiftVariantRequest = Schema<'GiftVariantRequest'>;

/**
 * Every Gifts write refreshes the whole module (web ADR 0009): both tabs, the settings and the home summary. A gift
 * with a giver and a buy-by day also has a to-do in the Todos module (gifts 002), so «Справи» refresh too.
 */
const giftQueries = [giftKeys.all, moduleKeys.todos];

const giftWriter: Writer<GiftRequest> = {
  keys: giftQueries,
  create: (body, key) =>
    apiCall(
      api.POST('/api/gifts/items', {
        params: { header: { 'Idempotency-Key': key } },
        body,
      }),
    ),
  update: (entry, body) =>
    apiCall(api.PUT('/api/gifts/items/{id}', { params: ifMatch(entry), body })),
  remove: (id) =>
    apiCall(api.DELETE('/api/gifts/items/{id}', { params: { path: { id } } })),
};

/** Adds or changes a gift with its whole variant list; refreshes both tabs and the summary. */
export const useSaveGift = () => useSave(giftWriter);
export const useDeleteGift = () => useRemove(giftWriter);

export interface StatusChange {
  gift: GiftItem;
  status: GiftStatus;
}

/**
 * «Подаровано», «Неактуально», «Повернути до списку»: moves a gift between the tabs with its version in If-Match. No
 * `giftedOn` is sent, so the server takes today in Kyiv for «Подаровано».
 */
export function useChangeGiftStatus() {
  const refresh = useRefresh(giftQueries);
  return useMutation({
    mutationFn: ({ gift, status }: StatusChange) =>
      apiCall(
        api.PUT('/api/gifts/items/{id}/status', {
          params: ifMatch(gift),
          body: { status, giftedOn: null },
        }),
      ),
    onSuccess: refresh,
    onError: (error) => {
      if (isStale(error)) refresh();
    },
  });
}
