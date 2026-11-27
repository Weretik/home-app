import Box from '@mui/material/Box';
import type { ShoppingSummary as Summary } from '../data/shopping-summary-api';
import { DepartmentsCard } from '../cards/DepartmentsCard';
import { FrequentCard } from '../cards/FrequentCard';
import { NextItemsCard } from '../cards/NextItemsCard';
import { SummaryKpis } from '../cards/SummaryKpis';
import { EmptyList } from './EmptyList';

/** The loaded summary: the figures, then the next items, departments and most bought, or «Список порожній». */
export function SummaryContent({ summary }: { summary: Summary }) {
  return (
    <>
      <SummaryKpis summary={summary} />
      {summary.neededCount === 0 ? (
        <EmptyList />
      ) : (
        <Box
          sx={{
            display: 'grid',
            gap: { xs: 2, md: 3 },
            gridTemplateColumns: {
              xs: '1fr',
              md: 'repeat(2, 1fr)',
              lg: 'repeat(3, 1fr)',
            },
            alignItems: 'start',
          }}
        >
          <NextItemsCard summary={summary} />
          <DepartmentsCard summary={summary} />
          <FrequentCard summary={summary} />
        </Box>
      )}
    </>
  );
}
