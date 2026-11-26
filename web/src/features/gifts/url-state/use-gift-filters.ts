import { useSearchParams } from 'react-router-dom';
import type { Recipient } from '../data/gifts-api';
import { parseFilters, type GiftFilters } from './gift-filters';

export interface GiftFilterState {
  filters: GiftFilters;
  isFiltered: boolean;
  setRecipient: (value: Recipient | undefined) => void;
  setCategory: (value: string | undefined) => void;
  /** Removes both filters and keeps any other param. */
  clear: () => void;
}

/**
 * The list filters in the URL (`?recipient=&category=`), so a link, a reload or «Назад» keeps them (web ADR 0006).
 * A change replaces the history entry instead of adding one. `categoryIds` drops a category that does not exist.
 */
export function useGiftFilters(
  categoryIds?: readonly string[],
): GiftFilterState {
  const [params, setParams] = useSearchParams();
  const filters = parseFilters(params, categoryIds);

  const update = (changes: Record<string, string | undefined>) =>
    setParams(
      (current) => {
        const next = new URLSearchParams(current);
        for (const [name, value] of Object.entries(changes)) {
          if (value) next.set(name, value);
          else next.delete(name);
        }
        return next;
      },
      { replace: true },
    );

  return {
    filters,
    isFiltered: Boolean(filters.recipient || filters.categoryId),
    setRecipient: (value) => update({ recipient: value }),
    setCategory: (value) => update({ category: value }),
    clear: () => update({ recipient: undefined, category: undefined }),
  };
}
