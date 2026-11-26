import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';
import type { GiftItem } from '../data/gifts-api';
import { recipientLabels } from '../display/labels';
import { groupByRecipient } from '../url-state/gift-filters';

interface GiftGroupsProps {
  items: GiftItem[];
  /** Groups per person with headings; otherwise one grid. */
  grouped: boolean;
  renderCard: (gift: GiftItem) => ReactNode;
}

const gridSx = {
  display: 'grid',
  gap: { xs: 2, md: 3 },
  gridTemplateColumns: { xs: '1fr', md: '1fr 1fr', xl: 'repeat(3, 1fr)' },
  alignItems: 'start',
} as const;

/** The wish cards: per person (Маша, Виталий, Вместе, each with its count) or as one grid. */
export function GiftGroups({ items, grouped, renderCard }: GiftGroupsProps) {
  if (!grouped) return <Box sx={gridSx}>{items.map(renderCard)}</Box>;

  return (
    <Box sx={{ display: 'grid', gap: { xs: 3, md: 4 } }}>
      {groupByRecipient(items).map((group) => (
        <Box
          component="section"
          key={group.recipient}
          aria-label={recipientLabels[group.recipient]}
        >
          <Typography
            variant="subtitle1"
            component="h2"
            sx={{ fontWeight: 600, mb: 1.5 }}
          >
            {recipientLabels[group.recipient]}{' '}
            <Box
              component="span"
              sx={{ color: 'text.secondary', fontWeight: 400 }}
            >
              {group.items.length}
            </Box>
          </Typography>
          <Box sx={gridSx}>{group.items.map(renderCard)}</Box>
        </Box>
      ))}
    </Box>
  );
}
