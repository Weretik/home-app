import OpenInNewOutlinedIcon from '@mui/icons-material/OpenInNewOutlined';
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
import { GiftNotes } from '../gift-rows/GiftNotes';
import { VariantLinks } from '../gift-rows/VariantLinks';

interface GiftListRowProps extends GiftHandlers {
  gift: GiftItem;
  variant: 'active' | 'archive';
  isPending: boolean;
  divider: boolean;
}

const chipRow = {
  display: 'flex',
  flexWrap: 'wrap',
  alignItems: 'center',
  gap: 0.75,
} as const;

/**
 * One gift as a phone row: the name alone at full width, then the price and category, the purchase line and notes,
 * and a last line of chips with the link, the variants toggle and «⋮».
 */
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
  const hasPrice = gift.price != null;

  return (
    <ListItem
      divider={divider}
      sx={{ display: 'grid', gap: 0.5, py: 1.5, px: 2 }}
    >
      <Typography
        sx={{ fontWeight: 600, fontSize: '1.05rem', overflowWrap: 'anywhere' }}
      >
        {gift.name}
      </Typography>
      <Typography variant="body2" color="text.secondary">
        <Box
          component="span"
          sx={{
            fontVariantNumeric: 'tabular-nums',
            fontWeight: hasPrice ? 600 : 400,
            color: hasPrice ? 'text.primary' : undefined,
          }}
        >
          {formatPrice(gift.price)}
        </Box>
        {' · '}
        <span>{gift.categoryName}</span>
      </Typography>
      <GiftNotes gift={gift} />
      <Box sx={{ ...chipRow, mt: 0.5 }}>
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
        {gift.link && (
          <Button
            size="small"
            href={gift.link}
            target="_blank"
            rel="noopener noreferrer"
            endIcon={<OpenInNewOutlinedIcon />}
            aria-label={`Відкрити посилання: ${gift.name}`}
            sx={{ py: 0 }}
          >
            Посилання
          </Button>
        )}
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
        <Box sx={{ ml: 'auto' }}>
          <GiftActions
            gift={gift}
            variant={variant}
            isPending={isPending}
            compact
            {...handlers}
          />
        </Box>
      </Box>
      <Collapse in={open} unmountOnExit>
        <VariantLinks id={variantsId} variants={gift.variants} />
      </Collapse>
    </ListItem>
  );
}
