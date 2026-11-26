import { useSearchParams } from 'react-router-dom';
import { monthDays, type Month } from '../data/month';
import type { Period } from './period';

const edges = ['from', 'to'] as const;

export interface PeriodParam {
  /** The page shows a period of days (`?from=&to=` present) instead of the month. */
  active: boolean;
  /** The dates as typed; empty while a field is cleared. */
  period: Period;
  /** Switches to a period, starting with the open month's first and last day. */
  start: (month: Month) => void;
  setEdge: (edge: keyof Period, value: string) => void;
  /** Goes back to the month (drops `from` and `to`). */
  end: () => void;
}

/**
 * The period of days the expense list shows instead of a month, kept in `?from=&to=` so a link or a reload keeps it.
 * The month (`year`, `month`) and the filters stay in the URL beside it.
 */
export function usePeriodParam(): PeriodParam {
  const [params, setParams] = useSearchParams();
  const active = edges.some((edge) => params.has(edge));
  const period = { from: params.get('from') ?? '', to: params.get('to') ?? '' };

  const update = (
    change: (updated: URLSearchParams) => void,
    replace = false,
  ) =>
    setParams(
      (current) => {
        const updated = new URLSearchParams(current);
        change(updated);
        return updated;
      },
      { replace },
    );

  return {
    active,
    period,
    start: (month) =>
      update((updated) => {
        const days = monthDays(month);
        for (const edge of edges) updated.set(edge, days[edge]);
      }),
    setEdge: (edge, value) =>
      update((updated) => updated.set(edge, value), true),
    end: () =>
      update((updated) => {
        for (const edge of edges) updated.delete(edge);
      }),
  };
}
