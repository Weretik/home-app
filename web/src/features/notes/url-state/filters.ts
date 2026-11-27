import type { NoteKind, NotePerson, NoteQuery } from '../data/notes-api';

/** The kind chip in the URL (`?kind=`): lower-case words, so a link reads well. */
export type KindFilter = 'likes' | 'dislikes' | 'facts';

export const kindFilters: readonly KindFilter[] = [
  'likes',
  'dislikes',
  'facts',
];

export const kindFilterLabels: Record<KindFilter, string> = {
  likes: 'Любить',
  dislikes: 'Не любить',
  facts: 'Факти',
};

const filterKind: Record<KindFilter, NoteKind> = {
  likes: 'Likes',
  dislikes: 'Dislikes',
  facts: 'Fact',
};

/** The filters of the list as the URL keeps them (FR-002). `category` is an id or `none`. */
export interface NoteFilters {
  kind?: KindFilter;
  category?: string;
  q?: string;
}

/** The filters from the URL; an unknown kind is ignored. */
export function readFilters(params: URLSearchParams): NoteFilters {
  const kind = params.get('kind') as KindFilter | null;
  return {
    kind: kind && kindFilters.includes(kind) ? kind : undefined,
    category: params.get('category') || undefined,
    q: params.get('q') || undefined,
  };
}

/** The API query of the tab's person and the filters (R-013). */
export function toNoteQuery(
  about: NotePerson | undefined,
  filters: NoteFilters,
): NoteQuery {
  const query: NoteQuery = {};
  if (about) query.about = about;
  if (filters.kind) query.kind = [filterKind[filters.kind]];
  if (filters.category === 'none') query.withoutCategory = true;
  else if (filters.category) query.categoryId = filters.category;
  const search = filters.q?.trim();
  if (search) query.search = search;
  return query;
}

/** The kind a quick add starts with: the filtered one, else «Любить» (FR-003). */
export const quickAddKind = (filters: NoteFilters): NoteKind =>
  filters.kind ? filterKind[filters.kind] : 'Likes';

export const isFiltered = (filters: NoteFilters) =>
  Boolean(filters.kind || filters.category || filters.q?.trim());
