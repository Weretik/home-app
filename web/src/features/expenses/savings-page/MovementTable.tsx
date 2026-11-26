import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import { formatDay, formatSignedAmount } from '../data/formatting';
import { personLabels } from '../data/labels';
import type { SavingsMovementItem } from '../data/savings-api';
import { RowActions } from '../entry-dialogs/RowActions';
import type { RowHandlers } from '../entry-editing/use-entry-editor';
import { SignedAmount } from './SignedAmount';

/** The month's movements of the jar as a table, for a computer. */
export function MovementTable({
  items,
  onEdit,
  onDelete,
}: { items: SavingsMovementItem[] } & RowHandlers<SavingsMovementItem>) {
  return (
    <TableContainer>
      <Table size="small" aria-label="Движения копилки за месяц">
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
              <TableCell align="right">
                <SignedAmount
                  value={item.amount}
                  variant="body2"
                  sx={{ fontWeight: 600 }}
                />
              </TableCell>
              <TableCell sx={{ color: 'text.secondary' }}>
                {item.comment}
              </TableCell>
              <TableCell padding="checkbox">
                <RowActions
                  label={`${personLabels[item.person]}, ${formatSignedAmount(item.amount)}`}
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
