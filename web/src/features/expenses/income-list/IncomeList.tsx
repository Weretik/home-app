import List from '@mui/material/List';
import Typography from '@mui/material/Typography';
import type { IncomeItem } from '../data/expenses-api';
import { formatAmount, formatDay } from '../data/formatting';
import { personLabels } from '../data/labels';
import { RowActions } from '../entry-dialogs/RowActions';
import type { RowHandlers } from '../entry-editing/use-entry-editor';
import { CompactRow } from '../month-view/CompactRow';

/** Incomes as a compact list, for a phone. */
export function IncomeList({
  items,
  onEdit,
  onDelete,
}: { items: IncomeItem[] } & RowHandlers<IncomeItem>) {
  return (
    <List disablePadding aria-label="Доходи за місяць">
      {items.map((item, index) => (
        <CompactRow
          key={item.id}
          divider={index < items.length - 1}
          title={personLabels[item.person]}
          amount={formatAmount(item.amount)}
          actions={
            <RowActions
              label={`${personLabels[item.person]}, ${formatAmount(item.amount)}`}
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
