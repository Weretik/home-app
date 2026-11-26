import ReceiptLongOutlinedIcon from '@mui/icons-material/ReceiptLongOutlined';
import { formatAmount } from '../data/formatting';
import { payerLabels } from '../data/labels';
import type { RowAction } from '../entry-dialogs/RowActions';
import type { RowHandlers } from '../entry-editing/use-entry-editor';
import type { PlannedExpenseItem } from '../planned-data/planned-api';

/** What the phone list and the computer table of plans take. */
export interface PlannedRowsProps extends RowHandlers<PlannedExpenseItem> {
  items: PlannedExpenseItem[];
  onRecord: (item: PlannedExpenseItem) => void;
}

/** «Записать как расход» in a plan's «⋮» menu. */
export const recordAction = (onClick: () => void): RowAction => ({
  label: 'Записать как расход',
  icon: <ReceiptLongOutlinedIcon fontSize="small" />,
  onClick,
});

/** A plan in one line: «Страховка · Машина · Виталий · 4 200,00 ₴». */
export const planLine = (item: PlannedExpenseItem) =>
  `${item.name} · ${item.categoryName} · ${payerLabels[item.payer]} · ${formatAmount(item.amount)}`;
