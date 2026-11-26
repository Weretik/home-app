import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import type { IncomeItem } from '../data/expenses-api';
import { formatAmount, formatDay } from '../data/formatting';
import { personLabels } from '../data/labels';
import { RowActions } from '../entry-dialogs/RowActions';
import type { RowHandlers } from '../entry-editing/use-entry-editor';

/** Incomes as a compact list, for a phone. */
export function IncomeList({
  items,
  onEdit,
  onDelete,
}: { items: IncomeItem[] } & RowHandlers<IncomeItem>) {
  return (
    <List disablePadding aria-label="Доходы за месяц">
      {items.map((item, index) => (
        <ListItem
          key={item.id}
          divider={index < items.length - 1}
          sx={{ alignItems: 'flex-start', gap: 2, py: 1.25 }}
        >
          <ListItemText
            sx={{ my: 0, minWidth: 0 }}
            primary={personLabels[item.person]}
            secondary={
              <>
                {formatDay(item.date)}
                {item.comment && (
                  <Typography
                    component="span"
                    variant="body2"
                    sx={{ display: 'block', overflowWrap: 'anywhere' }}
                  >
                    {item.comment}
                  </Typography>
                )}
              </>
            }
          />
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <Typography
              sx={{
                fontWeight: 600,
                whiteSpace: 'nowrap',
                fontVariantNumeric: 'tabular-nums',
              }}
            >
              {formatAmount(item.amount)}
            </Typography>
            <RowActions
              label={`${personLabels[item.person]}, ${formatAmount(item.amount)}`}
              onEdit={() => onEdit(item)}
              onDelete={() => onDelete(item)}
            />
          </Box>
        </ListItem>
      ))}
    </List>
  );
}
