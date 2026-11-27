import List from '@mui/material/List';
import Typography from '@mui/material/Typography';
import { formatDay, formatSignedAmount } from '../data/formatting';
import { personLabels } from '../data/labels';
import type { SavingsMovementItem } from '../data/savings-api';
import { RowActions } from '../entry-dialogs/RowActions';
import type { RowHandlers } from '../entry-editing/use-entry-editor';
import { CompactRow } from '../month-view/CompactRow';
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
        <CompactRow
          key={item.id}
          divider={index < items.length - 1}
          title={personLabels[item.person]}
          amount={
            <SignedAmount
              component="span"
              value={item.amount}
              sx={{ fontWeight: 600 }}
            />
          }
          actions={
            <RowActions
              label={`${personLabels[item.person]}, ${formatSignedAmount(item.amount)}`}
              onEdit={() => onEdit(item)}
              onDelete={() => onDelete(item)}
            />
          }
          details={
            <>
              {formatDay(item.date)}
              {item.comment && (
                <Typography
                  component="span"
                  variant="body2"
                  color="text.primary"
                  sx={{ display: 'block' }}
                >
                  {item.comment}
                </Typography>
              )}
            </>
          }
        />
      ))}
    </List>
  );
}
