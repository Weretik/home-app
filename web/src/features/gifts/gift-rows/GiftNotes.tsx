import Typography from '@mui/material/Typography';
import type { GiftItem } from '../data/gifts-api';
import { formatPurchase, kyivToday } from '../display/purchase';

/**
 * The lines under a gift's name: the purchase line of a wanted gift («🛒 купити до 12.12 · дарує Віталій», red when
 * late) and its notes in at most two lines.
 */
export function GiftNotes({ gift }: { gift: GiftItem }) {
  const purchase =
    gift.status === 'Wanted' ? formatPurchase(gift, kyivToday()) : null;
  return (
    <>
      {purchase && (
        <Typography
          variant="body2"
          sx={{
            color: purchase.overdue ? 'error.main' : 'text.secondary',
            fontWeight: purchase.overdue ? 600 : undefined,
          }}
        >
          {purchase.text}
        </Typography>
      )}
      {gift.notes && (
        <Typography
          variant="body2"
          color="text.secondary"
          title={gift.notes}
          sx={{
            display: '-webkit-box',
            WebkitLineClamp: 2,
            WebkitBoxOrient: 'vertical',
            overflow: 'hidden',
            overflowWrap: 'anywhere',
          }}
        >
          {gift.notes}
        </Typography>
      )}
    </>
  );
}
