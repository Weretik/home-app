import Box from '@mui/material/Box';
import Chip from '@mui/material/Chip';
import ListItem from '@mui/material/ListItem';
import Typography from '@mui/material/Typography';
import type { SpendingLimitItem } from '../budget-data/budget-api';
import { formatAmount } from '../data/formatting';
import { limitPeriodLabels } from '../data/labels';
import { RowActions } from '../entry-dialogs/RowActions';
import type { RowHandlers } from '../entry-editing/use-entry-editor';

interface LimitRowProps extends RowHandlers<SpendingLimitItem> {
  limit: SpendingLimitItem;
  /** Category names by id; «…» while they load. */
  categoryNames: Map<string, string>;
  divider: boolean;
}

/** One spending limit: its name and categories as chips, the amount per month or quarter, and the «⋮» menu. */
export function LimitRow({
  limit,
  categoryNames,
  divider,
  onEdit,
  onDelete,
}: LimitRowProps) {
  return (
    <ListItem
      divider={divider}
      sx={{ alignItems: 'flex-start', gap: 2, py: 1.5 }}
    >
      <Box sx={{ flex: 1, minWidth: 0, display: 'grid', gap: 1 }}>
        <Typography sx={{ fontWeight: 600, overflowWrap: 'anywhere' }}>
          {limit.name}
        </Typography>
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.75 }}>
          {limit.categoryIds.map((id) => (
            <Chip key={id} size="small" label={categoryNames.get(id) ?? '…'} />
          ))}
        </Box>
      </Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
        <Box sx={{ textAlign: 'right' }}>
          <Typography
            sx={{
              fontWeight: 600,
              whiteSpace: 'nowrap',
              fontVariantNumeric: 'tabular-nums',
            }}
          >
            {formatAmount(limit.amount)}
          </Typography>
          <Typography variant="caption" color="text.secondary">
            {limitPeriodLabels[limit.period]}
          </Typography>
        </Box>
        <RowActions
          label={`${limit.name}, ${formatAmount(limit.amount)}`}
          onEdit={() => onEdit(limit)}
          onDelete={() => onDelete(limit)}
        />
      </Box>
    </ListItem>
  );
}
