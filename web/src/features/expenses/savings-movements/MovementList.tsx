import Box from '@mui/material/Box';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Typography from '@mui/material/Typography';
import { formatDay, formatSignedAmount } from '../data/formatting';
import { personLabels } from '../data/labels';
import type { SavingsMovementItem } from '../data/savings-api';
import { RowActions } from '../entry-dialogs/RowActions';
import type { RowHandlers } from '../entry-editing/use-entry-editor';
import { SignedAmount } from '../savings-page/SignedAmount';

/** The month's movements of the jar as a compact list, for a phone. */
export function MovementList({
  items,
  onEdit,
  onDelete,
}: { items: SavingsMovementItem[] } & RowHandlers<SavingsMovementItem>) {
  return (
    <List disablePadding aria-label="Операції скарбнички за місяць">
      {items.map((item, index) => (
        <ListItem
          key={item.id}
          divider={index < items.length - 1}
          sx={{ alignItems: 'flex-start', gap: 2, py: 1.25 }}
        >
          <ListItemText
            sx={{ my: 0, minWidth: 0 }}
            primary={personLabels[item.person]}
            secondary={
              <>
                {formatDay(item.date)}
                {item.comment && (
                  <Typography
                    component="span"
                    variant="body2"
                    sx={{ display: 'block', overflowWrap: 'anywhere' }}
                  >
                    {item.comment}
                  </Typography>
                )}
              </>
            }
          />
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <SignedAmount value={item.amount} sx={{ fontWeight: 600 }} />
            <RowActions
              label={`${personLabels[item.person]}, ${formatSignedAmount(item.amount)}`}
              onEdit={() => onEdit(item)}
              onDelete={() => onDelete(item)}
            />
          </Box>
        </ListItem>
      ))}
    </List>
  );
}
