import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import type { LimitRow } from '../data/limit-rows';
import { formatExact } from '../data/money';
import { LimitFill } from './LimitFill';
import { leftColor, limitsLabel, number, orDash } from './limit-format';

/** «Лимиты» on a computer: a small table. */
export function LimitTable({ rows }: { rows: LimitRow[] }) {
  return (
    <TableContainer sx={{ pb: 1 }}>
      <Table size="small" aria-label={limitsLabel}>
        <TableHead>
          <TableRow>
            <TableCell>Название</TableCell>
            <TableCell align="right">Потрачено</TableCell>
            <TableCell align="right">Лимит</TableCell>
            <TableCell align="right">Остаток</TableCell>
            <TableCell align="right">%</TableCell>
            <TableCell aria-label="Заполнение" sx={{ width: '16%' }} />
            <TableCell align="right">Можно в день</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.map((row) => (
            <TableRow
              key={row.key}
              hover
              sx={row.isTotal ? { '& td': { fontWeight: 700 } } : undefined}
            >
              <TableCell>{row.name}</TableCell>
              <TableCell align="right" sx={number}>
                {formatExact(row.spent)}
              </TableCell>
              <TableCell align="right" sx={number}>
                {orDash(row.limit, formatExact)}
              </TableCell>
              <TableCell
                align="right"
                sx={{ ...number, color: leftColor(row.left) }}
              >
                {orDash(row.left, formatExact)}
              </TableCell>
              <TableCell align="right" sx={number}>
                {orDash(row.percent, (p) => `${p}%`)}
              </TableCell>
              <TableCell>
                {row.percent !== null && (
                  <LimitFill name={row.name} percent={row.percent} />
                )}
              </TableCell>
              <TableCell align="right" sx={number}>
                {orDash(row.perDay, formatExact)}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
