import List from '@mui/material/List';
import Typography from '@mui/material/Typography';
import type { ExpenseItem } from '../data/expenses-api';
import { formatAmount, formatDay } from '../data/formatting';
import { payerLabels, paymentMethodLabels } from '../data/labels';
import { RowActions } from '../entry-dialogs/RowActions';
import { CompactRow } from '../month-view/CompactRow';
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
  /** The list's accessible name: «Витрати за місяць» or «Витрати за період». */
  label: string;
} & RowHandlers<ExpenseItem>) {
  return (
    <List disablePadding aria-label={label}>
      {items.map((item, index) => (
        <CompactRow
          key={item.id}
          divider={index < items.length - 1}
          title={
            <>
              {item.categoryName}
              {item.outsideLimits && <OutsideLimitsChip />}
            </>
          }
          amount={formatAmount(item.amount)}
          actions={
            <RowActions
              label={`${item.categoryName}, ${formatAmount(item.amount)}`}
              onEdit={() => onEdit(item)}
              onDelete={() => onDelete(item)}
            />
          }
          details={
            <>
              {formatDay(item.date)} · {payerLabels[item.payer]} ·{' '}
              {paymentMethodLabels[item.paymentMethod]}
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
