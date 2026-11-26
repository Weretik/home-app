import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Typography from '@mui/material/Typography';
import type { ExpenseItem } from '../data/expenses-api';
import { formatAmount, formatDay } from '../data/formatting';
import { payerLabels, paymentMethodLabels } from '../data/labels';

/** Expenses as a compact list, for a phone. */
export function ExpenseList({ items }: { items: ExpenseItem[] }) {
  return (
    <List disablePadding aria-label="Расходы за месяц">
      {items.map((item, index) => (
        <ListItem
          key={item.id}
          divider={index < items.length - 1}
          sx={{ alignItems: 'flex-start', gap: 2, py: 1.25 }}
        >
          <ListItemText
            sx={{ my: 0, minWidth: 0 }}
            primary={item.categoryName}
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
          <Typography
            sx={{
              fontWeight: 600,
              whiteSpace: 'nowrap',
              fontVariantNumeric: 'tabular-nums',
            }}
          >
            {formatAmount(item.amount)}
          </Typography>
        </ListItem>
      ))}
    </List>
  );
}
