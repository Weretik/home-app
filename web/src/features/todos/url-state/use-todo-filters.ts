import { useEffect, useRef, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useSignedInPerson } from '../../../platform/client/member-api';
import type { TaskView } from '../data/todos-api';
import { whenFits, type When } from './date-filter';
import {
  everyoneParam,
  resolveWho,
  type TodoFilters,
  type Who,
} from './filters';

const names = ['who', 'category', 'q', 'when', 'from', 'to'] as const;
type Name = (typeof names)[number];

/**
 * The filters of a task list kept in `?who=&category=&q=&when=&from=&to=`, so a link opens the same list. A `when`
 * the list does not offer (another list's choice) is ignored. Without `who` the list shows the signed-in person;
 * «Усі» and «Скинути фільтр» write `who=all` (identity 002 FR-002).
 */
export function useTodoFilters(view: TaskView = 'active') {
  const [params, setParams] = useSearchParams();
  const person = useSignedInPerson();
  const rawWhen = (params.get('when') || undefined) as When | undefined;
  const when = whenFits(view, rawWhen) ? rawWhen : undefined;
  const filters: TodoFilters = {
    who: resolveWho(params.get('who'), person),
    category: params.get('category') || undefined,
    q: params.get('q') || undefined,
    when,
    from: when === 'period' ? params.get('from') || undefined : undefined,
    to: when === 'period' ? params.get('to') || undefined : undefined,
  };

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

  const clear = () =>
    setParams(
      (current) => {
        const updated = new URLSearchParams(current);
        for (const name of names) updated.delete(name);
        updated.set('who', everyoneParam);
        return updated;
      },
      { replace: true },
    );

  return {
    filters,
    setWho: (who: Who | undefined) => set({ who: who ?? everyoneParam }),
    setCategory: (category: string | undefined) => set({ category }),
    setSearch: (q: string | undefined) => set({ q }),
    // A new choice drops the old period's days.
    setWhen: (value: When | undefined) =>
      set({ when: value, from: undefined, to: undefined }),
    setPeriod: (from: string | undefined, to: string | undefined) =>
      set({ when: 'period', from, to }),
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
  // The URL changed from outside (a «Скинути фільтр»): show it.
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
