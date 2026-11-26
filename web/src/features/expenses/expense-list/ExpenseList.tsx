import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import type { ExpenseItem } from '../data/expenses-api';
import { formatAmount, formatDay } from '../data/formatting';
import { payerLabels, paymentMethodLabels } from '../data/labels';
import { RowActions } from '../entry-dialogs/RowActions';
import { OutsideLimitsChip } from './OutsideLimitsChip';
import type { RowHandlers } from '../entry-editing/use-entry-editor';

/** Expenses as a compact list, for a phone. */
export function ExpenseList({
  items,
  label,
  onEdit,
  onDelete,
}: {
  items: ExpenseItem[];
  /** The list's accessible name: «Расходы за месяц» or «Расходы за период». */
  label: string;
} & RowHandlers<ExpenseItem>) {
  return (
    <List disablePadding aria-label={label}>
      {items.map((item, index) => (
        <ListItem
          key={item.id}
          divider={index < items.length - 1}
          sx={{ alignItems: 'flex-start', gap: 2, py: 1.25 }}
        >
          <ListItemText
            sx={{ my: 0, minWidth: 0 }}
            primary={
              <>
                {item.categoryName}
                {item.outsideLimits && <OutsideLimitsChip />}
              </>
            }
            secondary={
              <>
                {formatDay(item.date)} · {payerLabels[item.payer]} ·{' '}
                {paymentMethodLabels[item.paymentMethod]}
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
            <Typography
              sx={{
                fontWeight: 600,
                whiteSpace: 'nowrap',
                fontVariantNumeric: 'tabular-nums',
              }}
            >
              {formatAmount(item.amount)}
            </Typography>
            <RowActions
              label={`${item.categoryName}, ${formatAmount(item.amount)}`}
              onEdit={() => onEdit(item)}
              onDelete={() => onDelete(item)}
            />
          </Box>
        </ListItem>
      ))}
    </List>
  );
}
