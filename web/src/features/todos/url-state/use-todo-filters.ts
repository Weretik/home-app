import { useEffect, useRef, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { whoValues, type TodoFilters, type Who } from './filters';

const names = ['who', 'category', 'q'] as const;

/** The filters of a task list kept in `?who=&category=&q=`, so a link opens the same list. */
export function useTodoFilters() {
  const [params, setParams] = useSearchParams();
  const rawWho = params.get('who');
  const filters: TodoFilters = {
    who: whoValues.includes(rawWho as Who) ? (rawWho as Who) : undefined,
    category: params.get('category') || undefined,
    q: params.get('q') || undefined,
  };

  const set = (name: (typeof names)[number], value: string | undefined) =>
    setParams(
      (current) => {
        const updated = new URLSearchParams(current);
        if (value) updated.set(name, value);
        else updated.delete(name);
        return updated;
      },
      { replace: true },
    );

  const clear = () =>
    setParams(
      (current) => {
        const updated = new URLSearchParams(current);
        for (const name of names) updated.delete(name);
        return updated;
      },
      { replace: true },
    );

  return {
    filters,
    setWho: (who: Who | undefined) => set('who', who),
    setCategory: (category: string | undefined) => set('category', category),
    setSearch: (q: string | undefined) => set('q', q),
    clear,
  };
}

/** A search field's text, written to the URL 300 ms after the last key press (FR-002). */
export function useDebouncedSearch(
  value: string | undefined,
  onChange: (value: string | undefined) => void,
) {
  const [text, setText] = useState(value ?? '');
  const [synced, setSynced] = useState(value);
  // The URL changed from outside (a «Сбросить фильтр»): show it.
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
