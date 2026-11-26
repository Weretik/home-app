import Box from '@mui/material/Box';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import Typography from '@mui/material/Typography';
import type { LimitRow } from '../data/limit-rows';
import { formatExact } from '../data/money';
import { LimitFill } from './LimitFill';
import { leftColor, limitsLabel, number } from './limit-format';

/** «Лимиты» on a phone: the name and the percent, the bar, then «X из Y · остаток Z · в день W». */
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
            <Typography
              variant="body2"
              color="text.secondary"
              sx={{ fontVariantNumeric: 'tabular-nums' }}
            >
              <Box component="span" sx={number}>
                {formatExact(row.spent)} из {formatExact(row.limit)}
              </Box>
              {' · '}
              <Box
                component="span"
                sx={{ ...number, color: leftColor(row.left) }}
              >
                остаток {formatExact(row.left)}
              </Box>
              {row.perDay !== null && (
                <>
                  {' · '}
                  <Box component="span" sx={number}>
                    в день {formatExact(row.perDay)}
                  </Box>
                </>
              )}
            </Typography>
          )}
        </ListItem>
      ))}
    </List>
  );
}
