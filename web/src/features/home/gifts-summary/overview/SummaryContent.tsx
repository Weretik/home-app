import Box from '@mui/material/Box';
import type { GiftsSummaryView } from '../data/summary-api';
import { DetailsCard } from '../cards/DetailsCard';
import { OccasionsCard } from '../cards/OccasionsCard';
import { RecipientCards } from '../cards/RecipientCards';
import { TopWishesCard } from '../cards/TopWishesCard';
import { NoWishes } from './NoWishes';

/** The loaded summary: the figures, then the 🔥 wishes and the occasions (or «Желаний пока нет») and the «нюансы». */
export function SummaryContent({ summary }: { summary: GiftsSummaryView }) {
  return (
    <>
      <RecipientCards summary={summary} />
      <Box
        sx={{
          display: 'grid',
          gap: { xs: 2, md: 3 },
          gridTemplateColumns: {
            xs: '1fr',
            md: '1fr 1fr',
            lg: 'repeat(3, 1fr)',
          },
          alignItems: 'start',
        }}
      >
        {summary.total.wantedCount === 0 ? (
          <NoWishes />
        ) : (
          <>
            <TopWishesCard summary={summary} />
            <OccasionsCard summary={summary} />
          </>
        )}
        <DetailsCard summary={summary} />
      </Box>
    </>
  );
}
