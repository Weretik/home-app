import Card from '@mui/material/Card';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Typography from '@mui/material/Typography';
import type { ShoppingSummary } from '../data/shopping-summary-api';
import { times } from '../data/summary-labels';

/** «Купуємо найчастіше»: the top five names of the frequent window. */
export function FrequentCard({ summary }: { summary: ShoppingSummary }) {
  if (summary.topFrequent.length === 0) return null;
  return (
    <Card component="section" aria-labelledby="shopping-frequent-title">
      <Typography
        id="shopping-frequent-title"
        variant="subtitle1"
        component="h2"
        sx={{ fontWeight: 600, px: 2, pt: 2 }}
      >
        Купуємо найчастіше
      </Typography>
      <List aria-labelledby="shopping-frequent-title" dense>
        {summary.topFrequent.map((item) => (
          <ListItem
            key={item.name}
            secondaryAction={
              <Typography
                variant="body2"
                sx={{
                  color: 'text.secondary',
                  fontVariantNumeric: 'tabular-nums',
                }}
              >
                {times(item.timesBought)}
              </Typography>
            }
          >
            <ListItemText primary={`⭐ ${item.name}`} />
          </ListItem>
        ))}
      </List>
    </Card>
  );
}
