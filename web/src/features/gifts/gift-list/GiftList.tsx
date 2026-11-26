import Box from '@mui/material/Box';
import List from '@mui/material/List';
import { useId } from 'react';
import type { GiftItem } from '../data/gifts-api';
import { recipientLabels } from '../display/labels';
import { toneColor, toneTint } from '../display/recipient-tones';
import { GroupTitle } from '../gift-rows/GroupTitle';
import type { GiftRowsProps } from '../gift-table/GiftTable';
import { groupByRecipient, type GiftGroup } from '../url-state/gift-filters';
import { GiftListRow } from './GiftListRow';

type RowsProps = Omit<GiftRowsProps, 'items' | 'grouped'>;

function Rows({
  items,
  label,
  variant,
  isChanging,
  ...handlers
}: RowsProps & { items: GiftItem[] }) {
  return (
    <List disablePadding aria-label={label}>
      {items.map((gift, index) => (
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

function Group({
  group,
  ...props
}: RowsProps & { group: GiftGroup<GiftItem> }) {
  const titleId = useId();
  return (
    <Box
      component="section"
      aria-labelledby={titleId}
      sx={(theme) => ({
        borderRadius: 2,
        overflow: 'hidden',
        border: 1,
        borderColor: 'divider',
        boxShadow: `inset 4px 0 0 ${toneColor(theme, group.recipient)}`,
      })}
    >
      <Box
        sx={(theme) => ({
          px: 2,
          py: 1.25,
          bgcolor: toneTint(theme, group.recipient),
        })}
      >
        <GroupTitle
          id={titleId}
          recipient={group.recipient}
          count={group.items.length}
          priceSum={group.priceSum}
        />
      </Box>
      <Rows
        {...props}
        label={`${props.label}: ${recipientLabels[group.recipient]}`}
        items={group.items}
      />
    </Box>
  );
}

/** Gifts as a compact list, for a phone, in a coloured block per person; every action is in the row's «⋮». */
export function GiftList({ items, grouped, ...props }: GiftRowsProps) {
  if (!grouped) return <Rows {...props} items={items} />;
  return (
    <Box sx={{ display: 'grid', gap: 2 }}>
      {groupByRecipient(items).map((group) => (
        <Group key={group.recipient} group={group} {...props} />
      ))}
    </Box>
  );
}
