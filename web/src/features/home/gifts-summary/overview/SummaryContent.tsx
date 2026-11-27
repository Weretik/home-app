import Box from '@mui/material/Box';
import { DetailsCard } from '../cards/DetailsCard';
import { OccasionsCard } from '../cards/OccasionsCard';
import { RecipientCards } from '../cards/RecipientCards';
import type { GiftsSummaryView } from '../data/summary-api';
import { TopWishesCard } from '../top-wishes/TopWishesCard';
import { NoWishes } from './NoWishes';

/**
 * The loaded summary: the figures, «🔥 Дуже хочу» across the full width, then the occasions (or «Бажань поки немає»)
 * and the «нюанси» side by side from `md`.
 */
export function SummaryContent({ summary }: { summary: GiftsSummaryView }) {
  const hasWishes = summary.total.wantedCount > 0;
  return (
    <>
      <RecipientCards summary={summary} />
      {hasWishes && <TopWishesCard summary={summary} />}
      <Box
        sx={{
          display: 'grid',
          gap: { xs: 2, md: 3 },
          gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' },
          alignItems: 'start',
        }}
      >
        {hasWishes ? <OccasionsCard summary={summary} /> : <NoWishes />}
        <DetailsCard summary={summary} />
      </Box>
    </>
  );
}
