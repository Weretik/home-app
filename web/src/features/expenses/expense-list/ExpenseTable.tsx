import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import type { ExpenseItem } from '../data/expenses-api';
import { formatAmount, formatDay } from '../data/formatting';
import { payerLabels, paymentMethodLabels } from '../data/labels';

const amountCell = {
  whiteSpace: 'nowrap',
  fontVariantNumeric: 'tabular-nums',
} as const;

/** Expenses as a table, for a computer. */
export function ExpenseTable({ items }: { items: ExpenseItem[] }) {
  return (
    <TableContainer>
      <Table size="small" aria-label="Расходы за месяц">
        <TableHead>
          <TableRow>
            <TableCell>Дата</TableCell>
            <TableCell>Категория</TableCell>
            <TableCell>Кто</TableCell>
            <TableCell align="right">Сумма</TableCell>
            <TableCell>Оплата</TableCell>
            <TableCell>Комментарий</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {items.map((item) => (
            <TableRow key={item.id} hover>
              <TableCell sx={{ whiteSpace: 'nowrap' }}>
                {formatDay(item.date)}
              </TableCell>
              <TableCell>{item.categoryName}</TableCell>
              <TableCell>{payerLabels[item.payer]}</TableCell>
              <TableCell align="right" sx={{ ...amountCell, fontWeight: 600 }}>
                {formatAmount(item.amount)}
              </TableCell>
              <TableCell>{paymentMethodLabels[item.paymentMethod]}</TableCell>
              <TableCell sx={{ color: 'text.secondary' }}>
                {item.comment}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
