import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';
import { formatWhole, formatWishes } from '../data/formatting';
import { recipientLabels } from '../data/labels';
import type {
  GiftCounts,
  GiftsSummaryView,
  Recipient,
} from '../data/summary-api';
import { PersonAvatar } from './PersonAvatar';
import { personColors } from './person-colors';

function FigureCard({
  mark,
  title,
  counts,
  recipient,
}: {
  mark: ReactNode;
  title: string;
  counts: GiftCounts;
  recipient?: Recipient;
}) {
  return (
    <Card
      component="section"
      aria-label={title}
      sx={{
        p: 2.5,
        ...(recipient && {
          borderTop: '4px solid',
          borderTopColor: `${personColors[recipient]}.main`,
        }),
      }}
    >
      <Typography
        variant="subtitle1"
        component="h2"
        sx={{
          fontWeight: 600,
          color: 'text.secondary',
          display: 'flex',
          alignItems: 'center',
          gap: 1,
        }}
      >
        {mark}
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

/** The top row: one figure card per person in their colour and «Всего» (docs/design.md, «Figure card»). */
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
        title="Всего"
        counts={summary.total}
      />
    </Box>
  );
}
