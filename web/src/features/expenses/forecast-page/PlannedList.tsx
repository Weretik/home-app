import Box from '@mui/material/Box';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Typography from '@mui/material/Typography';
import { formatAmount } from '../data/formatting';
import { payerLabels } from '../data/labels';
import { RowActions } from '../entry-dialogs/RowActions';
import { recordAction, type PlannedRowsProps } from './planned-rows';

/** Planned expenses as a compact list, for a phone. */
export function PlannedList({
  items,
  onEdit,
  onDelete,
  onRecord,
}: PlannedRowsProps) {
  return (
    <List disablePadding aria-label="Запланированные расходы">
      {items.map((item, index) => (
        <ListItem
          key={item.id}
          divider={index < items.length - 1}
          sx={{ alignItems: 'flex-start', gap: 2, py: 1.25 }}
        >
          <ListItemText
            sx={{ my: 0, minWidth: 0, overflowWrap: 'anywhere' }}
            primary={item.name}
            secondary={`${item.categoryName} · ${payerLabels[item.payer]}`}
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
              label={`${item.name}, ${formatAmount(item.amount)}`}
              onEdit={() => onEdit(item)}
              onDelete={() => onDelete(item)}
              extra={recordAction(() => onRecord(item))}
            />
          </Box>
        </ListItem>
      ))}
    </List>
  );
}
