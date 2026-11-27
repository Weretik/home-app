import List from '@mui/material/List';
import { formatAmount } from '../data/formatting';
import { payerLabels } from '../data/labels';
import { RowActions } from '../entry-dialogs/RowActions';
import { CompactRow } from '../month-view/CompactRow';
import { recordAction, type PlannedRowsProps } from './planned-rows';

/** Planned expenses as a compact list, for a phone. */
export function PlannedList({
  items,
  onEdit,
  onDelete,
  onRecord,
}: PlannedRowsProps) {
  return (
    <List disablePadding aria-label="Заплановані витрати">
      {items.map((item, index) => (
        <CompactRow
          key={item.id}
          divider={index < items.length - 1}
          title={item.name}
          amount={formatAmount(item.amount)}
          actions={
            <RowActions
              label={`${item.name}, ${formatAmount(item.amount)}`}
              onEdit={() => onEdit(item)}
              onDelete={() => onDelete(item)}
              extra={recordAction(() => onRecord(item))}
            />
          }
          details={`${item.categoryName} · ${payerLabels[item.payer]}`}
        />
      ))}
    </List>
  );
}
