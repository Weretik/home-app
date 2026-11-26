import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import { formatAmount } from '../data/formatting';
import { payerLabels } from '../data/labels';
import { RowActions } from '../entry-dialogs/RowActions';
import { recordAction, type PlannedRowsProps } from './planned-rows';

/** Planned expenses as a table, for a computer. */
export function PlannedTable({
  items,
  onEdit,
  onDelete,
  onRecord,
}: PlannedRowsProps) {
  return (
    <TableContainer>
      <Table size="small" aria-label="Запланированные расходы">
        <TableHead>
          <TableRow>
            <TableCell>Название</TableCell>
            <TableCell>Категория</TableCell>
            <TableCell>Кто</TableCell>
            <TableCell align="right">Сумма</TableCell>
            <TableCell padding="checkbox" aria-label="Действия" />
          </TableRow>
        </TableHead>
        <TableBody>
          {items.map((item) => (
            <TableRow key={item.id} hover>
              <TableCell>{item.name}</TableCell>
              <TableCell>{item.categoryName}</TableCell>
              <TableCell>{payerLabels[item.payer]}</TableCell>
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
              <TableCell padding="checkbox">
                <RowActions
                  label={`${item.name}, ${formatAmount(item.amount)}`}
                  onEdit={() => onEdit(item)}
                  onDelete={() => onDelete(item)}
                  extra={recordAction(() => onRecord(item))}
                />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
