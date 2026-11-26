import { useSearchParams } from 'react-router-dom';

/**
 * A list filter kept in one search param, so a link keeps it. `allowed` limits the values (an enum); without it any
 * non-empty value is taken. An empty value removes the param.
 */
export function useFilterParam<T extends string>(
  name: string,
  allowed?: readonly T[],
): [T | undefined, (value: T | undefined) => void] {
  const [params, setParams] = useSearchParams();
  const raw = params.get(name) ?? '';
  const value =
    raw !== '' && (!allowed || (allowed as readonly string[]).includes(raw))
      ? (raw as T)
      : undefined;

  const setValue = (next: T | undefined) =>
    setParams(
      (current) => {
        const updated = new URLSearchParams(current);
        if (next) updated.set(name, next);
        else updated.delete(name);
        return updated;
      },
      { replace: true },
    );

  return [value, setValue];
}

/** Removes every filter of the page and keeps the month (`year`, `month`). */
export function useClearFilters(): () => void {
  const [, setParams] = useSearchParams();
  return () =>
    setParams(
      (current) => {
        const kept = new URLSearchParams();
        for (const name of ['year', 'month']) {
          const value = current.get(name);
          if (value) kept.set(name, value);
        }
        return kept;
      },
      { replace: true },
    );
}
