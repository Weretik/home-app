import { useMutation } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/api';
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

/** A gift write changes both tabs and the home summary. */
/** Every Gifts write refreshes the whole module (web ADR 0009): both tabs, the settings and the home summary. */
const giftQueries = [giftKeys.all];

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
 * «Подарено», «Не актуально», «Вернуть в список»: moves a gift between the tabs with its version in If-Match. No
 * `giftedOn` is sent, so the server takes today in Kyiv for «Подарено».
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
