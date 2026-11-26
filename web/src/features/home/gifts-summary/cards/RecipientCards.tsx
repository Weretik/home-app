import Box from '@mui/material/Box';
import { recipientEmoji, recipientLabels } from '../data/labels';
import type { GiftsSummaryView } from '../data/summary-api';
import { FigureCard } from './FigureCard';

/** The top row: one figure card per person and «Всего» (docs/design.md, «Figure card»). */
export function RecipientCards({ summary }: { summary: GiftsSummaryView }) {
  return (
    <Box
      sx={{
        display: 'grid',
        gap: { xs: 2, md: 3 },
        gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', lg: 'repeat(4, 1fr)' },
      }}
    >
      {summary.recipients.map((item) => (
        <FigureCard
          key={item.recipient}
          emoji={recipientEmoji[item.recipient]}
          title={recipientLabels[item.recipient]}
          counts={item.counts}
        />
      ))}
      <FigureCard emoji="🎁" title="Всего" counts={summary.total} />
    </Box>
  );
}
