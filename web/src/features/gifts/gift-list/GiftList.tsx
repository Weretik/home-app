import Box from '@mui/material/Box';
import type { GiftRowsProps } from '../gift-table/GiftTable';
import { groupByRecipient } from '../url-state/gift-filters';
import { ListRows } from './ListRows';
import { PersonBlock } from './PersonBlock';

/** Gifts as a compact list, for a phone, in a coloured block per person; every action is in the row's «⋮». */
export function GiftList({ items, grouped, ...props }: GiftRowsProps) {
  if (!grouped) return <ListRows {...props} items={items} />;
  return (
    <Box sx={{ display: 'grid', gap: 2 }}>
      {groupByRecipient(items).map((group) => (
        <PersonBlock key={group.recipient} group={group} {...props} />
      ))}
    </Box>
  );
}
