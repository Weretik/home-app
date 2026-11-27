import { useEffect, useRef, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useMe } from '../data/use-me';
import { resolveTab, tabOptions, type NoteTab } from '../section/notes-tabs';
import { readFilters, type KindFilter } from './filters';

/** The params the filters own; the tab (`about`) is kept apart, so «Скинути фільтри» keeps it. */
const filterNames = ['kind', 'category', 'q'] as const;
type Name = 'about' | (typeof filterNames)[number];

/**
 * The tab and the filters of «Про нас», kept in `?about=&kind=&category=&q=` so a link opens the same list (FR-001,
 * FR-002). The tab depends on the signed-in member, so `isLoading` holds the list back until the member is known.
 */
export function useNoteFilters() {
  const [params, setParams] = useSearchParams();
  const me = useMe();
  const tab = resolveTab(params.get('about'), me.person);

  const set = (values: Partial<Record<Name, string | undefined>>) =>
    setParams(
      (current) => {
        const updated = new URLSearchParams(current);
        for (const [name, value] of Object.entries(values)) {
          if (value) updated.set(name, value);
          else updated.delete(name);
        }
        return updated;
      },
      { replace: true },
    );

  return {
    me,
    tab,
    tabs: tabOptions(me.person),
    filters: readFilters(params),
    isLoading: me.isLoading,
    setTab: (value: NoteTab) => set({ about: value }),
    setKind: (kind: KindFilter | undefined) => set({ kind }),
    setCategory: (category: string | undefined) => set({ category }),
    setSearch: (q: string | undefined) => set({ q }),
    clear: () =>
      set(Object.fromEntries(filterNames.map((name) => [name, undefined]))),
  };
}

/** A search field's text, written to the URL 300 ms after the last key press (FR-002). */
export function useDebouncedSearch(
  value: string | undefined,
  onChange: (value: string | undefined) => void,
) {
  const [text, setText] = useState(value ?? '');
  const [synced, setSynced] = useState(value);
  // The URL changed from outside (a «Скинути фільтри»): show it.
  if (value !== synced) {
    setSynced(value);
    setText(value ?? '');
  }

  // The latest callback, so a re-render of the page does not restart the wait.
  const latest = useRef(onChange);
  useEffect(() => {
    latest.current = onChange;
  });

  useEffect(() => {
    if ((value ?? '') === text) return;
    const timer = setTimeout(() => latest.current(text || undefined), 300);
    return () => clearTimeout(timer);
  }, [text, value]);

  return [text, setText] as const;
}
