import Box from '@mui/material/Box';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import Typography from '@mui/material/Typography';
import { formatExact } from '../data/money';
import { LimitFill } from './LimitFill';
import { LimitNotes } from './LimitNotes';
import { leftColor, limitsLabel, numberSx, type LimitRow } from './limit-rows';

/** The limits on a phone: one list item per limit with its fill and sums, «ИТОГО» last. */
export function LimitList({ rows }: { rows: LimitRow[] }) {
  return (
    <List aria-label={limitsLabel} sx={{ pb: 1 }}>
      {rows.map((row) => (
        <ListItem
          key={row.key}
          divider={!row.isTotal}
          sx={{
            display: 'grid',
            gridTemplateColumns: 'minmax(0, 1fr)',
            gap: 0.75,
            px: 2,
          }}
        >
          <Box
            sx={{ display: 'flex', justifyContent: 'space-between', gap: 2 }}
          >
            <Typography
              sx={{ fontWeight: row.isTotal ? 700 : 600, minWidth: 0 }}
            >
              {row.name}
            </Typography>
            <Typography sx={{ ...numberSx, fontWeight: 600 }}>
              {row.percent === null
                ? formatExact(row.spent)
                : `${row.percent}%`}
            </Typography>
          </Box>
          {row.percent !== null && (
            <LimitFill name={row.name} percent={row.percent} />
          )}
          {row.limit !== null && row.left !== null && (
            <Typography
              variant="body2"
              color="text.secondary"
              sx={{ fontVariantNumeric: 'tabular-nums' }}
            >
              <Box component="span" sx={numberSx}>
                {formatExact(row.spent)} из {formatExact(row.limit)}
              </Box>
              {' · '}
              <Box
                component="span"
                sx={{ ...numberSx, color: leftColor(row.left) }}
              >
                остаток {formatExact(row.left)}
              </Box>
              {row.perDay !== null && (
                <>
                  {' · '}
                  <Box component="span" sx={numberSx}>
                    в день {formatExact(row.perDay)}
                  </Box>
                </>
              )}
            </Typography>
          )}
          <LimitNotes row={row} />
        </ListItem>
      ))}
    </List>
  );
}
