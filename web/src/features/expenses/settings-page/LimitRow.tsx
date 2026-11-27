import Box from '@mui/material/Box';
import Chip from '@mui/material/Chip';
import ListItem from '@mui/material/ListItem';
import Typography from '@mui/material/Typography';
import type { SpendingLimitItem } from '../budget-data/budget-api';
import { formatAmount } from '../data/formatting';
import { limitPeriodLabels } from '../data/labels';
import { RowActions } from '../entry-dialogs/RowActions';
import type { RowHandlers } from '../entry-editing/use-entry-editor';
import { earlierAmountCaption, nextAmountCaption } from './amount-steps';

interface LimitRowProps extends RowHandlers<SpendingLimitItem> {
  limit: SpendingLimitItem;
  /** Category names by id; «…» while they load. */
  categoryNames: Map<string, string>;
  divider: boolean;
}

/**
 * One spending limit: its name and categories as chips, today's amount per month or quarter (with since when and the
 * amount before, once it changed, and a change saved ahead), and the «⋮» menu.
 */
export function LimitRow({
  limit,
  categoryNames,
  divider,
  onEdit,
  onDelete,
}: LimitRowProps) {
  const captions = [
    earlierAmountCaption(limit.amounts),
    nextAmountCaption(limit.amounts),
  ].filter(Boolean);
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
          {captions.map((caption) => (
            <Typography
              key={caption}
              variant="caption"
              color="text.secondary"
              sx={{ display: 'block' }}
            >
              {caption}
            </Typography>
          ))}
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
