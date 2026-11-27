import ListItem from '@mui/material/ListItem';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';

interface CompactRowProps {
  /** The row's name: the category, the person or the plan; may hold a chip after it. */
  title: ReactNode;
  /** The amount, already formatted (a signed amount brings its own colour). */
  amount: ReactNode;
  /** The «⋮» menu. */
  actions: ReactNode;
  /** The caption lines (date, who, comment); they take the whole width of the row under the title. */
  details?: ReactNode;
  divider: boolean;
}

/**
 * One row of a phone list (docs/design.md, «Sections with several pages and month lists»): the title, the amount and
 * «⋮» on the first line, then the details across the whole row, so a comment is never squeezed into a narrow column
 * next to the amount (spec platform/002, R-M-002).
 */
export function CompactRow({
  title,
  amount,
  actions,
  details,
  divider,
}: CompactRowProps) {
  return (
    <ListItem
      divider={divider}
      sx={{
        display: 'grid',
        gridTemplateColumns: 'minmax(0, 1fr) auto auto',
        alignItems: 'center',
        columnGap: 1,
        rowGap: 0.25,
        py: 1,
        pr: 1,
      }}
    >
      <Typography component="div" sx={{ overflowWrap: 'anywhere' }}>
        {title}
      </Typography>
      <Typography
        component="div"
        sx={{
          fontWeight: 600,
          whiteSpace: 'nowrap',
          fontVariantNumeric: 'tabular-nums',
        }}
      >
        {amount}
      </Typography>
      {actions}
      {details && (
        <Typography
          component="div"
          variant="body2"
          color="text.secondary"
          sx={{ gridColumn: '1 / -1', overflowWrap: 'anywhere' }}
        >
          {details}
        </Typography>
      )}
    </ListItem>
  );
}
