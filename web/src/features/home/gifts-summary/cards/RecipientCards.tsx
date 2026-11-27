import Box from '@mui/material/Box';
import { recipientLabels } from '../data/labels';
import type { GiftsSummaryView } from '../data/summary-api';
import { PersonAvatar } from '../persons/PersonAvatar';
import { FigureCard } from './FigureCard';

/** The top row: one figure card per person in their colour and «Усього» (docs/design.md, «Figure card»). */
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
          mark={<PersonAvatar recipient={item.recipient} size={28} />}
          title={recipientLabels[item.recipient]}
          counts={item.counts}
          recipient={item.recipient}
        />
      ))}
      <FigureCard
        mark={
          <Box component="span" aria-hidden>
            🎁
          </Box>
        }
        title="Усього"
        counts={summary.total}
      />
    </Box>
  );
}
