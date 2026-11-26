import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';
import { formatWhole, formatWishes } from '../data/formatting';
import { recipientEmoji, recipientLabels } from '../data/labels';
import type { GiftCounts, GiftsSummaryView } from '../data/summary-api';

function FigureCard({
  emoji,
  title,
  counts,
}: {
  emoji: string;
  title: string;
  counts: GiftCounts;
}) {
  return (
    <Card component="section" aria-label={title} sx={{ p: 2.5 }}>
      <Typography
        variant="subtitle1"
        component="h2"
        sx={{ fontWeight: 600, color: 'text.secondary' }}
      >
        <Box component="span" aria-hidden sx={{ mr: 1 }}>
          {emoji}
        </Box>
        {title}
      </Typography>
      <Typography
        variant="h4"
        component="p"
        sx={{ my: 0.5, fontVariantNumeric: 'tabular-nums' }}
      >
        {formatWishes(counts.wantedCount)}
      </Typography>
      <Typography
        variant="body2"
        color="text.secondary"
        sx={{ fontVariantNumeric: 'tabular-nums' }}
      >
        ≈ {formatWhole(counts.wantedPriceSum)} · {counts.wantedWithoutPrice} без
        цены
      </Typography>
      <Typography
        variant="body2"
        color="text.secondary"
        sx={{ fontVariantNumeric: 'tabular-nums' }}
      >
        🔥 {counts.highCount} · подарено {counts.giftedCount} на{' '}
        {formatWhole(counts.giftedPriceSum)}
      </Typography>
    </Card>
  );
}

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
