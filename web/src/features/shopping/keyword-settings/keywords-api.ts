import {
  keepPreviousData,
  useQuery,
  queryOptions,
} from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/client/api';
import {
  ifMatch,
  useShoppingWrite,
  type Versioned,
} from '../data/item-writes-api';
import { shoppingKey } from '../data/shopping-api';

export type Keyword = Schema<'KeywordItem'>;
export type KeywordRequest = Schema<'UpdateShoppingKeywordRequest'>;

export interface KeywordFilter {
  departmentId?: string;
  search?: string;
}

/** The query of `useKeywords`, also used to load it ahead for offline use (web ADR 0022). */
export const keywordsQuery = (filter: KeywordFilter) =>
  queryOptions({
    queryKey: [...shoppingKey, 'keywords', filter],
    queryFn: ({ signal }) =>
      apiCall(
        api.GET('/api/shopping/keywords', {
          params: { query: filter },
          signal,
        }),
      ),
  });

/** The keywords that pick a department, filtered on the server; the old rows stay while a new search loads. */
export function useKeywords(filter: KeywordFilter) {
  return useQuery({
    ...keywordsQuery(filter),
    placeholderData: keepPreviousData,
  });
}

export interface SaveKeywordInput {
  body: KeywordRequest;
  edit?: Versioned;
  /** Sent with a create (one per opened form). */
  idempotencyKey: string;
}

/** Adds a keyword or changes it and its department. */
export const useSaveKeyword = () =>
  useShoppingWrite(({ body, edit, idempotencyKey }: SaveKeywordInput) =>
    edit
      ? apiCall(
          api.PUT('/api/shopping/keywords/{id}', {
            params: ifMatch(edit),
            body,
          }),
        )
      : apiCall(
          api.POST('/api/shopping/keywords', {
            params: { header: { 'Idempotency-Key': idempotencyKey } },
            body,
          }),
        ),
  );

/** Deletes a keyword. */
export const useDeleteKeyword = () =>
  useShoppingWrite((row: Versioned) =>
    apiCall(
      api.DELETE('/api/shopping/keywords/{id}', { params: ifMatch(row) }),
    ),
  );
