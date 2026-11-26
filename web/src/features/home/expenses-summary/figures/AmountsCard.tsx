import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import Typography from '@mui/material/Typography';
import { formatExact } from '../data/money';
import type { AmountLine } from './card-lines';

const number = { whiteSpace: 'nowrap', fontVariantNumeric: 'tabular-nums' };

/** A small card with a title and name / amount lines. */
export function AmountsCard({
  title,
  lines,
  empty,
}: {
  title: string;
  lines: AmountLine[];
  empty: string;
}) {
  return (
    <Card component="section" aria-label={title} sx={{ p: { xs: 2, md: 2.5 } }}>
      <Typography variant="subtitle1" component="h2" sx={{ fontWeight: 600 }}>
        {title}
      </Typography>
      {lines.length === 0 ? (
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
          {empty}
        </Typography>
      ) : (
        <List dense aria-label={title}>
          {lines.map((line) => (
            <ListItem
              key={line.key}
              disableGutters
              sx={{ display: 'flex', gap: 2, justifyContent: 'space-between' }}
            >
              <Box sx={{ minWidth: 0 }}>
                <Typography>{line.name}</Typography>
                {line.caption && (
                  <Typography variant="body2" color="text.secondary">
                    {line.caption}
                  </Typography>
                )}
              </Box>
              <Typography sx={{ ...number, fontWeight: 600 }}>
                {formatExact(line.amount)}
              </Typography>
            </ListItem>
          ))}
        </List>
      )}
    </Card>
  );
}
