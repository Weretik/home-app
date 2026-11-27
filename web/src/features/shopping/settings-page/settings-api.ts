import { useQuery, queryOptions } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/client/api';
import { useShoppingWrite } from '../data/item-writes-api';
import { shoppingKey } from '../data/shopping-api';

export type ShoppingSettings = Schema<'ShoppingSettingsView'>;
export type ShoppingSettingsRequest = Schema<'UpdateShoppingSettingsRequest'>;

/** The query of `useShoppingSettings`, also used to load it ahead for offline use (web ADR 0022). */
export const shoppingSettingsQuery = () =>
  queryOptions({
    queryKey: [...shoppingKey, 'settings'],
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/shopping/settings', { signal })),
  });

/** How long a bought item stays in the cart and how far back ⭐ Часті look. */
export function useShoppingSettings() {
  return useQuery({ ...shoppingSettingsQuery() });
}

/** Saves the list settings with the version the form started from. */
export const useSaveShoppingSettings = () =>
  useShoppingWrite(
    ({ version, body }: { version: number; body: ShoppingSettingsRequest }) =>
      apiCall(
        api.PUT('/api/shopping/settings', {
          params: { header: { 'If-Match': String(version) } },
          body,
        }),
      ),
  );
