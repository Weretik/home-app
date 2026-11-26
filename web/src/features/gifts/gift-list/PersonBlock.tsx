import Box from '@mui/material/Box';
import { useId } from 'react';
import type { GiftItem } from '../data/gifts-api';
import { recipientLabels } from '../display/labels';
import { toneColor, toneTint } from '../display/recipient-tones';
import { GroupTitle } from '../gift-rows/GroupTitle';
import type { GiftGroup } from '../url-state/gift-filters';
import { ListRows, type ListRowsProps } from './ListRows';

interface PersonBlockProps extends Omit<ListRowsProps, 'items'> {
  group: GiftGroup<GiftItem>;
}

/** One person's gifts on a phone: a tinted header in their colour, then their rows, with a line in the colour. */
export function PersonBlock({ group, ...props }: PersonBlockProps) {
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
      <ListRows
        {...props}
        label={`${props.label}: ${recipientLabels[group.recipient]}`}
        items={group.items}
      />
    </Box>
  );
}
