import ExpandLessOutlinedIcon from '@mui/icons-material/ExpandLessOutlined';
import ExpandMoreOutlinedIcon from '@mui/icons-material/ExpandMoreOutlined';
import Button from '@mui/material/Button';
import TableCell from '@mui/material/TableCell';
import TableRow from '@mui/material/TableRow';
import { useId, useState } from 'react';
import type { GiftItem } from '../data/gifts-api';
import {
  formatOccasion,
  formatOutcome,
  formatPrice,
} from '../display/formatting';
import { priorityLabels } from '../display/labels';
import { toneColor } from '../display/recipient-tones';
import { GiftActions, type GiftHandlers } from '../gift-rows/GiftActions';
import { GiftName } from '../gift-rows/GiftName';
import { VariantLinks } from '../gift-rows/VariantLinks';

interface GiftTableRowProps extends GiftHandlers {
  gift: GiftItem;
  variant: 'active' | 'archive';
  isPending: boolean;
  /** How many columns the variants row spans. */
  columns: number;
}

/** One gift as a table row; its variants open in a row under it. */
export function GiftTableRow({
  gift,
  variant,
  isPending,
  columns,
  ...handlers
}: GiftTableRowProps) {
  const [open, setOpen] = useState(false);
  const variantsId = useId();
  const count = gift.variants.length;
  const isActive = variant === 'active';

  return (
    <>
      <TableRow hover sx={open ? { '& > td': { borderBottom: 0 } } : undefined}>
        <TableCell
          sx={(theme) => ({
            minWidth: 200,
            boxShadow: `inset 4px 0 0 ${toneColor(theme, gift.recipient)}`,
          })}
        >
          <GiftName gift={gift} />
        </TableCell>
        <TableCell>
          {count > 0 && (
            <Button
              size="small"
              aria-label={`Варианты: ${count}`}
              aria-expanded={open}
              aria-controls={open ? variantsId : undefined}
              onClick={() => setOpen((value) => !value)}
              endIcon={
                open ? <ExpandLessOutlinedIcon /> : <ExpandMoreOutlinedIcon />
              }
              sx={{ minWidth: 0 }}
            >
              {count}
            </Button>
          )}
        </TableCell>
        <TableCell>{gift.categoryName}</TableCell>
        {isActive && (
          <TableCell sx={{ whiteSpace: 'nowrap' }}>
            {priorityLabels[gift.priority]}
          </TableCell>
        )}
        <TableCell
          align="right"
          sx={{
            whiteSpace: 'nowrap',
            fontVariantNumeric: 'tabular-nums',
            fontWeight: gift.price == null ? 400 : 600,
            color: gift.price == null ? 'text.secondary' : 'text.primary',
          }}
        >
          {formatPrice(gift.price)}
        </TableCell>
        <TableCell sx={{ color: isActive ? 'text.primary' : 'text.secondary' }}>
          {isActive
            ? formatOccasion(gift.occasion, gift.occasionDate)
            : formatOutcome(gift)}
        </TableCell>
        <TableCell padding="checkbox" sx={{ pr: 1 }}>
          <GiftActions
            gift={gift}
            variant={variant}
            isPending={isPending}
            {...handlers}
          />
        </TableCell>
      </TableRow>
      {open && (
        <TableRow>
          <TableCell colSpan={columns} sx={{ pt: 0, pl: 4 }}>
            <VariantLinks id={variantsId} variants={gift.variants} />
          </TableCell>
        </TableRow>
      )}
    </>
  );
}
