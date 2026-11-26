import EventOutlinedIcon from '@mui/icons-material/EventOutlined';
import Box from '@mui/material/Box';
import Chip from '@mui/material/Chip';
import type { GiftItem } from '../data/gifts-api';
import { formatLongDay, formatOccasion } from '../display/formatting';
import { priorityLabels, recipientLabels } from '../display/labels';

interface GiftChipsProps {
  gift: GiftItem;
  /** Name the recipient too, where no group heading or filter says it. */
  showRecipient?: boolean;
}

/** The facts of a wish as chips: where it ended up, recipient, priority, category, occasion with its date. */
export function GiftChips({ gift, showRecipient }: GiftChipsProps) {
  const occasion = formatOccasion(gift.occasion, gift.occasionDate);

  return (
    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.75 }}>
      {gift.status === 'Gifted' && (
        <Chip
          size="small"
          color="primary"
          variant="outlined"
          label={
            gift.giftedOn
              ? `Подарено ${formatLongDay(gift.giftedOn)}`
              : 'Подарено'
          }
        />
      )}
      {gift.status === 'Dropped' && (
        <Chip size="small" variant="outlined" label="Не актуально" />
      )}
      {showRecipient && (
        <Chip size="small" label={recipientLabels[gift.recipient]} />
      )}
      <Chip
        size="small"
        color={gift.priority === 'High' ? 'secondary' : 'default'}
        variant={gift.priority === 'High' ? 'outlined' : 'filled'}
        label={priorityLabels[gift.priority]}
      />
      <Chip size="small" label={gift.categoryName} />
      {occasion && (
        <Chip
          size="small"
          variant="outlined"
          icon={<EventOutlinedIcon fontSize="small" />}
          label={occasion}
        />
      )}
    </Box>
  );
}
