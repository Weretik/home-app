import Card from '@mui/material/Card';
import Chip from '@mui/material/Chip';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Typography from '@mui/material/Typography';
import type { ShoppingSummary } from '../data/shopping-summary-api';
import { itemLine, personName } from '../data/summary-labels';

/** «Купити насамперед»: up to eight needed items, urgent first. */
export function NextItemsCard({ summary }: { summary: ShoppingSummary }) {
  return (
    <Card component="section" aria-labelledby="shopping-next-title">
      <Typography
        id="shopping-next-title"
        variant="subtitle1"
        component="h2"
        sx={{ fontWeight: 600, px: 2, pt: 2 }}
      >
        Купити насамперед
      </Typography>
      <List aria-labelledby="shopping-next-title" dense>
        {summary.nextItems.map((item) => (
          <ListItem
            key={item.id}
            secondaryAction={
              item.urgent ? (
                <Chip size="small" color="secondary" label="Терміново" />
              ) : undefined
            }
          >
            <ListItemText
              primary={itemLine(item)}
              secondary={`${item.departmentIcon ?? ''} ${item.departmentName} · ${personName(item.addedBy)}`.trim()}
              slotProps={{ primary: { sx: { fontWeight: 600 } } }}
            />
          </ListItem>
        ))}
      </List>
    </Card>
  );
}
