import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import Collapse from '@mui/material/Collapse';
import ListItem from '@mui/material/ListItem';
import Typography from '@mui/material/Typography';
import { useId, useState } from 'react';
import type { GiftItem } from '../data/gifts-api';
import {
  formatOccasion,
  formatOutcome,
  formatPrice,
  formatVariantsCount,
} from '../display/formatting';
import { priorityLabels } from '../display/labels';
import { GiftActions, type GiftHandlers } from '../gift-rows/GiftActions';
import { GiftName } from '../gift-rows/GiftName';
import { VariantLinks } from '../gift-rows/VariantLinks';

interface GiftListRowProps extends GiftHandlers {
  gift: GiftItem;
  variant: 'active' | 'archive';
  isPending: boolean;
  divider: boolean;
}

/** One gift as a compact phone row: name and price, person and category, small chips, the variants toggle, «⋮». */
export function GiftListRow({
  gift,
  variant,
  isPending,
  divider,
  ...handlers
}: GiftListRowProps) {
  const [open, setOpen] = useState(false);
  const variantsId = useId();
  const isActive = variant === 'active';
  const occasion = isActive
    ? formatOccasion(gift.occasion, gift.occasionDate)
    : undefined;

  return (
    <ListItem
      divider={divider}
      sx={{ display: 'grid', gap: 0.75, py: 1.25, px: 2 }}
    >
      <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1 }}>
        <Box sx={{ flex: 1, minWidth: 0 }}>
          <GiftName gift={gift} />
          <Typography variant="body2" color="text.secondary">
            {gift.categoryName}
          </Typography>
        </Box>
        <Typography
          sx={{
            pt: 0.25,
            whiteSpace: 'nowrap',
            fontVariantNumeric: 'tabular-nums',
            fontWeight: gift.price == null ? 400 : 600,
          }}
          variant={gift.price == null ? 'body2' : 'body1'}
          color={gift.price == null ? 'text.secondary' : 'text.primary'}
        >
          {formatPrice(gift.price)}
        </Typography>
        <GiftActions
          gift={gift}
          variant={variant}
          isPending={isPending}
          compact
          {...handlers}
        />
      </Box>
      <Box
        sx={{
          display: 'flex',
          flexWrap: 'wrap',
          alignItems: 'center',
          gap: 0.75,
        }}
      >
        {isActive ? (
          <Chip
            size="small"
            label={priorityLabels[gift.priority]}
            variant="outlined"
            sx={{ fontWeight: gift.priority === 'High' ? 600 : 400 }}
          />
        ) : (
          <Chip size="small" variant="outlined" label={formatOutcome(gift)} />
        )}
        {occasion && <Chip size="small" variant="outlined" label={occasion} />}
        {gift.variants.length > 0 && (
          <Button
            size="small"
            aria-expanded={open}
            aria-controls={open ? variantsId : undefined}
            onClick={() => setOpen((value) => !value)}
            sx={{ py: 0 }}
          >
            {formatVariantsCount(gift.variants.length)}
          </Button>
        )}
      </Box>
      <Collapse in={open} unmountOnExit>
        <VariantLinks id={variantsId} variants={gift.variants} />
      </Collapse>
    </ListItem>
  );
}
