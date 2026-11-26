import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import type { Theme } from '@mui/material/styles';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Typography from '@mui/material/Typography';
import useMediaQuery from '@mui/material/useMediaQuery';
import { limitRows, type LimitRow } from '../data/limit-rows';
import { formatExact } from '../data/money';
import type { MonthSummary } from '../data/summary-api';
import { LimitFill } from './LimitFill';

const label = 'Лимиты месяца';
const number = { whiteSpace: 'nowrap', fontVariantNumeric: 'tabular-nums' };
const leftColor = (left: number | null) =>
  left !== null && left < 0 ? 'error.main' : 'inherit';
const orDash = (value: number | null, format: (v: number) => string) =>
  value === null ? '—' : format(value);

/** «Лимиты»: every spending limit of the month, then «ИТОГО» against the monthly limit. */
export function LimitsCard({ summary }: { summary: MonthSummary }) {
  const isComputer = useMediaQuery((theme: Theme) =>
    theme.breakpoints.up('md'),
  );
  const rows = limitRows(summary);

  return (
    <Card component="section" aria-labelledby="limits-title">
      <Typography
        id="limits-title"
        variant="subtitle1"
        component="h2"
        sx={{ fontWeight: 600, px: { xs: 2, md: 3 }, pt: { xs: 2, md: 2.5 } }}
      >
        Лимиты
      </Typography>
      {summary.limits.length === 0 && (
        <Typography
          variant="body2"
          color="text.secondary"
          sx={{ px: { xs: 2, md: 3 }, pt: 0.5 }}
        >
          Лимитов по категориям пока нет; ниже весь месяц целиком.
        </Typography>
      )}
      {isComputer ? <LimitTable rows={rows} /> : <LimitList rows={rows} />}
    </Card>
  );
}

function LimitTable({ rows }: { rows: LimitRow[] }) {
  return (
    <TableContainer sx={{ pb: 1 }}>
      <Table size="small" aria-label={label}>
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

function LimitList({ rows }: { rows: LimitRow[] }) {
  return (
    <List aria-label={label} sx={{ pb: 1 }}>
      {rows.map((row) => (
        <ListItem
          key={row.key}
          divider={!row.isTotal}
          sx={{ display: 'grid', gap: 0.75, px: 2 }}
        >
          <Box
            sx={{ display: 'flex', justifyContent: 'space-between', gap: 2 }}
          >
            <Typography sx={{ fontWeight: row.isTotal ? 700 : 600 }}>
              {row.name}
            </Typography>
            <Typography sx={{ ...number, fontWeight: 600 }}>
              {row.percent === null
                ? formatExact(row.spent)
                : `${row.percent}%`}
            </Typography>
          </Box>
          {row.percent !== null && (
            <LimitFill name={row.name} percent={row.percent} />
          )}
          {row.limit !== null && row.left !== null && (
            <Typography variant="body2" color="text.secondary" sx={number}>
              {formatExact(row.spent)} из {formatExact(row.limit)} ·{' '}
              <Box component="span" sx={{ color: leftColor(row.left) }}>
                остаток {formatExact(row.left)}
              </Box>
              {row.perDay !== null && ` · в день ${formatExact(row.perDay)}`}
            </Typography>
          )}
        </ListItem>
      ))}
    </List>
  );
}
