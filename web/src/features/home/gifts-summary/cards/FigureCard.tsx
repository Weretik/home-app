import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';
import { formatWhole, formatWishes } from '../data/formatting';
import type { GiftCounts, Recipient } from '../data/summary-api';
import { personColors } from '../persons/person-colors';

interface FigureCardProps {
  /** The person's circle, or an emoji for «Усього». */
  mark: ReactNode;
  title: string;
  counts: GiftCounts;
  /** A person's card gets a 4px top border in their colour. */
  recipient?: Recipient;
}

/** One person's (or «Усього») wishes: how many, their sum, how many 🔥, and what was given. */
export function FigureCard({
  mark,
  title,
  counts,
  recipient,
}: FigureCardProps) {
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
        ціни
      </Typography>
      <Typography
        variant="body2"
        color="text.secondary"
        sx={{ fontVariantNumeric: 'tabular-nums' }}
      >
        🔥 {counts.highCount} · подаровано {counts.giftedCount} на{' '}
        {formatWhole(counts.giftedPriceSum)}
      </Typography>
    </Card>
  );
}
