import { useSearchParams } from 'react-router-dom';
import { currentMonth, parseMonth, type Month } from './month';

/**
 * The month the expenses summary shows, kept in `?year=&month=` like the Expenses pages, so a link opens the same
 * month. Without them it is the current month. Changing it keeps the other params (the open summary).
 */
export function useMonthParam(): [Month, (month: Month) => void] {
  const [params, setParams] = useSearchParams();
  const month = parseMonth(
    params.get('year'),
    params.get('month'),
    currentMonth(),
  );

  const setMonth = (next: Month) =>
    setParams((current) => {
      const updated = new URLSearchParams(current);
      updated.set('year', String(next.year));
      updated.set('month', String(next.month));
      return updated;
    });

  return [month, setMonth];
}
