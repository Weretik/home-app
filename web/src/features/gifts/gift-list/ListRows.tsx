import List from '@mui/material/List';
import type { GiftItem } from '../data/gifts-api';
import type { GiftRowsProps } from '../gift-table/GiftTable';
import { GiftListRow } from './GiftListRow';

export type ListRowsProps = Omit<GiftRowsProps, 'grouped'>;

/** One plain list of compact rows, named by `label`. */
export function ListRows({
  items,
  label,
  variant,
  isChanging,
  ...handlers
}: ListRowsProps) {
  return (
    <List disablePadding aria-label={label}>
      {items.map((gift: GiftItem, index) => (
        <GiftListRow
          key={gift.id}
          gift={gift}
          variant={variant}
          isPending={isChanging(gift)}
          divider={index < items.length - 1}
          {...handlers}
        />
      ))}
    </List>
  );
}
