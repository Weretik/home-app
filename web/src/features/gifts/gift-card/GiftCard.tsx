import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';
import { useId } from 'react';
import type { GiftItem } from '../data/gifts-api';
import { formatPrice } from '../display/formatting';
import { CardActions, type GiftHandlers } from './CardActions';
import { GiftChips } from './GiftChips';
import { VariantLinks } from './VariantLinks';

interface GiftCardProps extends GiftHandlers {
  gift: GiftItem;
  variant: 'active' | 'archive';
  isPending?: boolean;
  showRecipient?: boolean;
}

/** One wish (docs/design.md, «Wish cards»): name and price, its chips, notes, variants, then the actions. */
export function GiftCard({
  gift,
  variant,
  isPending,
  showRecipient,
  ...handlers
}: GiftCardProps) {
  const titleId = useId();

  return (
    <Card
      component="article"
      aria-labelledby={titleId}
      sx={{ p: 2, display: 'grid', gap: 1.25, alignContent: 'start' }}
    >
      <Box
        sx={{
          display: 'flex',
          alignItems: 'baseline',
          justifyContent: 'space-between',
          gap: 2,
        }}
      >
        <Typography
          id={titleId}
          variant="subtitle1"
          component="h3"
          sx={{ fontWeight: 600, overflowWrap: 'anywhere' }}
        >
          {gift.name}
        </Typography>
        <Typography
          sx={{
            whiteSpace: 'nowrap',
            fontVariantNumeric: 'tabular-nums',
            fontWeight: gift.price == null ? 400 : 600,
            color: gift.price == null ? 'text.secondary' : 'text.primary',
          }}
          variant={gift.price == null ? 'body2' : 'body1'}
        >
          {formatPrice(gift.price)}
        </Typography>
      </Box>
      <GiftChips gift={gift} showRecipient={showRecipient} />
      {gift.notes && (
        <Typography
          variant="body2"
          color="text.secondary"
          sx={{ whiteSpace: 'pre-line', overflowWrap: 'anywhere' }}
        >
          {gift.notes}
        </Typography>
      )}
      <VariantLinks variants={gift.variants} />
      <CardActions
        gift={gift}
        variant={variant}
        isPending={isPending}
        {...handlers}
      />
    </Card>
  );
}
