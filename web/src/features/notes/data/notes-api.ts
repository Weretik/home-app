import { useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/api';
import { moduleKeys } from '../../../platform/query-client';

export type NoteItem = Schema<'NoteItem'>;
/** Whom a note is about (and who wrote it): «Віталій» or «Маша». */
export type NotePerson = NoteItem['about'];
export type NoteKind = NoteItem['kind'];

/**
 * Everything the «Про нас» screens read sits under the module prefix (`moduleKeys.notes`), so one invalidation after
 * a write refreshes them all (web ADR 0009).
 */
export const notesKey = moduleKeys.notes;
export const noteItemsKey = [...moduleKeys.notes, 'items'] as const;

/** The list query the tab and the filters turn into (url-state/filters.ts). */
export interface NoteQuery {
  about?: NotePerson;
  kind?: NoteKind[];
  categoryId?: string;
  withoutCategory?: boolean;
  search?: string;
}

/**
 * The notes of a tab in the API's order: by category, important first, then kind and title (R-012). `enabled` waits
 * for the signed-in member, so the default tab is known before the first request.
 */
export function useNotes(query: NoteQuery, enabled = true) {
  return useQuery({
    queryKey: [...noteItemsKey, query],
    enabled,
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/notes/items', { params: { query }, signal })),
  });
}
