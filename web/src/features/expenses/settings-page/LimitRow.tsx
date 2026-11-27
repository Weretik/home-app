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
 * One spending limit, stacked so nothing is squeezed on a phone (spec platform/002, R-M-002): the name with «⋮»,
 * then today's amount per month or quarter (with since when and the amount before, once it changed, and a change
 * saved ahead), then the categories as chips.
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
      sx={{
        display: 'grid',
        gridTemplateColumns: 'minmax(0, 1fr) auto',
        alignItems: 'start',
        columnGap: 1,
        rowGap: 0.75,
        py: 1.5,
        pr: 1,
      }}
    >
      <Typography sx={{ fontWeight: 600, overflowWrap: 'anywhere', pt: 1 }}>
        {limit.name}
      </Typography>
      <RowActions
        label={`${limit.name}, ${formatAmount(limit.amount)}`}
        onEdit={() => onEdit(limit)}
        onDelete={() => onDelete(limit)}
      />
      <Box sx={{ gridColumn: '1 / -1' }}>
        <Typography
          component="span"
          sx={{
            fontWeight: 600,
            fontVariantNumeric: 'tabular-nums',
            whiteSpace: 'nowrap',
          }}
        >
          {formatAmount(limit.amount)}
        </Typography>{' '}
        <Typography component="span" variant="body2" color="text.secondary">
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
      <Box
        sx={{
          gridColumn: '1 / -1',
          display: 'flex',
          flexWrap: 'wrap',
          gap: 0.75,
        }}
      >
        {limit.categoryIds.map((id) => (
          <Chip key={id} size="small" label={categoryNames.get(id) ?? '…'} />
        ))}
      </Box>
    </ListItem>
  );
}
