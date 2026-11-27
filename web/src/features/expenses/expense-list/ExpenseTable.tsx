import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import type { ExpenseItem } from '../data/expenses-api';
import { formatAmount, formatDay } from '../data/formatting';
import { payerLabels, paymentMethodLabels } from '../data/labels';
import { RowActions } from '../entry-dialogs/RowActions';
import { OutsideLimitsChip } from './OutsideLimitsChip';
import type { RowHandlers } from '../entry-editing/use-entry-editor';

const amountCell = {
  whiteSpace: 'nowrap',
  fontVariantNumeric: 'tabular-nums',
} as const;

/** Expenses as a table, for a computer. */
export function ExpenseTable({
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
    <TableContainer>
      <Table size="small" aria-label={label}>
        <TableHead>
          <TableRow>
            <TableCell>Дата</TableCell>
            <TableCell>Категорія</TableCell>
            <TableCell>Хто</TableCell>
            <TableCell align="right">Сума</TableCell>
            <TableCell>Оплата</TableCell>
            <TableCell>Коментар</TableCell>
            <TableCell padding="checkbox" aria-label="Дії" />
          </TableRow>
        </TableHead>
        <TableBody>
          {items.map((item) => (
            <TableRow key={item.id} hover>
              <TableCell sx={{ whiteSpace: 'nowrap' }}>
                {formatDay(item.date)}
              </TableCell>
              <TableCell>
                {item.categoryName}
                {item.outsideLimits && <OutsideLimitsChip />}
              </TableCell>
              <TableCell>{payerLabels[item.payer]}</TableCell>
              <TableCell align="right" sx={{ ...amountCell, fontWeight: 600 }}>
                {formatAmount(item.amount)}
              </TableCell>
              <TableCell>{paymentMethodLabels[item.paymentMethod]}</TableCell>
              <TableCell sx={{ color: 'text.secondary' }}>
                {item.comment}
              </TableCell>
              <TableCell padding="checkbox">
                <RowActions
                  label={`${item.categoryName}, ${formatAmount(item.amount)}`}
                  onEdit={() => onEdit(item)}
                  onDelete={() => onDelete(item)}
                />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
