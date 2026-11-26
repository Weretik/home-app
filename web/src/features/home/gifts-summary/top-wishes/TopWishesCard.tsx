import Box from '@mui/material/Box';
import type { GiftsSummaryView } from '../data/summary-api';
import { SummaryCard } from '../cards/SummaryCard';
import { PersonWishes } from './PersonWishes';

/**
 * «🔥 Очень хочу», full width: one tinted block per person (side by side from `md`, stacked on a phone), each with up
 * to five most wanted gifts. Hidden while nobody has a 🔥 wish.
 */
export function TopWishesCard({ summary }: { summary: GiftsSummaryView }) {
  if (summary.recipients.every((item) => item.topWanted.length === 0))
    return null;

  return (
    <SummaryCard title="🔥 Очень хочу">
      <Box
        sx={{
          display: 'grid',
          gap: { xs: 1.5, md: 2 },
          gridTemplateColumns: {
            xs: '1fr',
            md: `repeat(${summary.recipients.length}, minmax(0, 1fr))`,
          },
          alignItems: 'stretch',
        }}
      >
        {summary.recipients.map((group) => (
          <PersonWishes key={group.recipient} group={group} />
        ))}
      </Box>
    </SummaryCard>
  );
}
