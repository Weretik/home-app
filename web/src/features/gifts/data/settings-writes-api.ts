import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/api';
import { giftKeys } from './gifts-api';
import { ifMatch, useRemove, useSave, type Writer } from './writes-api';

export type GiftCategoryRequest = Schema<'GiftCategoryRequest'>;
export type GiftDetailRequest = Schema<'GiftDetailRequest'>;

/** Gifts carry their category's name, so a category write refreshes them too. */
const categoryWriter: Writer<GiftCategoryRequest> = {
  keys: [giftKeys.all],
  create: (body, key) =>
    apiCall(
      api.POST('/api/gifts/categories', {
        params: { header: { 'Idempotency-Key': key } },
        body,
      }),
    ),
  // Categories carry no version: a rename sends no If-Match.
  update: (entry, body) =>
    apiCall(
      api.PUT('/api/gifts/categories/{id}', {
        params: { path: { id: entry.id } },
        body,
      }),
    ),
  remove: (id) =>
    apiCall(
      api.DELETE('/api/gifts/categories/{id}', { params: { path: { id } } }),
    ),
};

/** The home summary shows the «нюансы», so a detail write refreshes it too. */
const detailWriter: Writer<GiftDetailRequest> = {
  keys: [giftKeys.all],
  create: (body, key) =>
    apiCall(
      api.POST('/api/gifts/details', {
        params: { header: { 'Idempotency-Key': key } },
        body,
      }),
    ),
  update: (entry, body) =>
    apiCall(
      api.PUT('/api/gifts/details/{id}', { params: ifMatch(entry), body }),
    ),
  remove: (id) =>
    apiCall(
      api.DELETE('/api/gifts/details/{id}', { params: { path: { id } } }),
    ),
};

export const useSaveCategory = () => useSave(categoryWriter);
export const useDeleteCategory = () => useRemove(categoryWriter);
export const useSaveDetail = () => useSave(detailWriter);
export const useDeleteDetail = () => useRemove(detailWriter);
