import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import type { Recipient } from '../data/gifts-api';
import { formatGiftsCount, formatWhole } from '../display/formatting';
import { recipientLabels } from '../display/labels';
import { RecipientAvatar } from '../display/RecipientAvatar';

interface GroupTitleProps {
  id?: string;
  recipient: Recipient;
  count: number;
  /** The sum of the known prices; left out when zero. */
  priceSum: number;
}

/** A person's group header: their circle, the name, «3 подарка · ≈ 4 500 ₴». */
export function GroupTitle({
  id,
  recipient,
  count,
  priceSum,
}: GroupTitleProps) {
  const figures = [
    formatGiftsCount(count),
    priceSum > 0 ? `≈ ${formatWhole(priceSum)}` : undefined,
  ];
  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
      <RecipientAvatar recipient={recipient} />
      <Typography
        id={id}
        variant="subtitle1"
        component="h2"
        sx={{ fontWeight: 700 }}
      >
        {recipientLabels[recipient]}
      </Typography>
      <Typography
        variant="body2"
        color="text.secondary"
        sx={{ fontVariantNumeric: 'tabular-nums' }}
      >
        {figures.filter(Boolean).join(' · ')}
      </Typography>
    </Box>
  );
}
