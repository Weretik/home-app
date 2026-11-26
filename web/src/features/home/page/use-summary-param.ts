import { useSearchParams } from 'react-router-dom';
import { summarySections, type SummarySection } from './summary-sections';

const built = summarySections.filter((s) => s.Summary !== null);

/**
 * The summary the home page shows, kept in `?s=` so a link opens it. A missing, unknown or not yet built key gives the
 * first built summary. Switching keeps the other params (the month).
 */
export function useSummaryParam(): [SummarySection, (key: string) => void] {
  const [params, setParams] = useSearchParams();
  const active = built.find((s) => s.key === params.get('s')) ?? built[0]!;

  const setActive = (key: string) =>
    setParams((current) => {
      const updated = new URLSearchParams(current);
      updated.set('s', key);
      return updated;
    });

  return [active, setActive];
}
