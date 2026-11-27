import { useSearchParams } from 'react-router-dom';
import { useSignedInPerson } from '../../../platform/member-api';
import type { GiftPriority, Recipient } from '../data/gifts-api';
import { everyoneParam, parseFilters, type GiftFilters } from './gift-filters';

export interface GiftFilterState {
  filters: GiftFilters;
  isFiltered: boolean;
  setRecipient: (value: Recipient | undefined) => void;
  setCategory: (value: string | undefined) => void;
  setPriority: (value: GiftPriority | undefined) => void;
  /** Removes all the filters and keeps any other param. */
  clear: () => void;
}

/**
 * The list filters in the URL (`?recipient=&category=&priority=`), so a link, a reload or «Назад» keeps them (web ADR 0006).
 * A change replaces the history entry instead of adding one. `categoryIds` drops a category that does not exist.
 * Without `recipient` the list shows the signed-in person; «Усі» and «Скинути» write `recipient=all` (identity 002).
 */
export function useGiftFilters(
  categoryIds?: readonly string[],
): GiftFilterState {
  const [params, setParams] = useSearchParams();
  const filters = parseFilters(params, categoryIds, useSignedInPerson());

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
    isFiltered: Boolean(
      filters.recipient || filters.categoryId || filters.priority,
    ),
    setRecipient: (value) => update({ recipient: value ?? everyoneParam }),
    setCategory: (value) => update({ category: value }),
    setPriority: (value) => update({ priority: value }),
    clear: () =>
      update({
        recipient: everyoneParam,
        category: undefined,
        priority: undefined,
      }),
  };
}
