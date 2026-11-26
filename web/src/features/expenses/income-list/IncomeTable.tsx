import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import type { IncomeItem } from '../data/expenses-api';
import { formatAmount, formatDay } from '../data/formatting';
import { personLabels } from '../data/labels';
import { RowActions } from '../entry-dialogs/RowActions';
import type { RowHandlers } from '../entry-editing/use-entry-editor';

/** Incomes as a table, for a computer. */
export function IncomeTable({
  items,
  onEdit,
  onDelete,
}: { items: IncomeItem[] } & RowHandlers<IncomeItem>) {
  return (
    <TableContainer>
      <Table size="small" aria-label="Доходы за месяц">
        <TableHead>
          <TableRow>
            <TableCell>Дата</TableCell>
            <TableCell>Кто</TableCell>
            <TableCell align="right">Сумма</TableCell>
            <TableCell>Комментарий</TableCell>
            <TableCell padding="checkbox" aria-label="Действия" />
          </TableRow>
        </TableHead>
        <TableBody>
          {items.map((item) => (
            <TableRow key={item.id} hover>
              <TableCell sx={{ whiteSpace: 'nowrap' }}>
                {formatDay(item.date)}
              </TableCell>
              <TableCell>{personLabels[item.person]}</TableCell>
              <TableCell
                align="right"
                sx={{
                  whiteSpace: 'nowrap',
                  fontVariantNumeric: 'tabular-nums',
                  fontWeight: 600,
                }}
              >
                {formatAmount(item.amount)}
              </TableCell>
              <TableCell sx={{ color: 'text.secondary' }}>
                {item.comment}
              </TableCell>
              <TableCell padding="checkbox">
                <RowActions
                  label={`${personLabels[item.person]}, ${formatAmount(item.amount)}`}
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
