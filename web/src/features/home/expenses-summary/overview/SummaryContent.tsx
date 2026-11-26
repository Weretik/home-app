import { isEmptyMonth } from '../data/figure-lines';
import type { MonthSummary } from '../data/summary-api';
import { ForecastCard } from '../figures/ForecastCard';
import { KpiCards } from '../figures/KpiCards';
import { SpendingCards } from '../figures/SpendingCards';
import { LimitsCard } from '../limits/LimitsCard';
import { EmptyMonth } from './EmptyMonth';

/** The loaded month: the four figures always, then the limits, the forecast and who spent what, or the empty month. */
export function SummaryContent({ summary }: { summary: MonthSummary }) {
  return (
    <>
      <KpiCards summary={summary} />
      {isEmptyMonth(summary) ? (
        <EmptyMonth />
      ) : (
        <>
          <LimitsCard summary={summary} />
          <ForecastCard summary={summary} />
          <SpendingCards summary={summary} />
        </>
      )}
    </>
  );
}
