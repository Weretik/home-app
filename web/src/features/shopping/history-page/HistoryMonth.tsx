import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { useId } from 'react';
import type { ShoppingItem } from '../data/shopping-api';
import { HistoryDay } from './HistoryDay';
import { monthHeading, type HistoryMonth as Month } from './history-groups';

interface HistoryMonthProps {
  month: Month;
  /** The item being returned to the list: its button waits. */
  returning: string | null;
  onReturn: (item: ShoppingItem) => void;
  onDelete: (item: ShoppingItem) => void;
}

/** One month of the history: «Ноябрь 2026 · 38 покупок», then its day cards, newest first (FR-301, FR-302). */
export function HistoryMonth({ month, ...actions }: HistoryMonthProps) {
  const titleId = useId();
  return (
    <Box
      component="section"
      aria-labelledby={titleId}
      sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}
    >
      <Typography
        id={titleId}
        variant="h6"
        component="h2"
        sx={{ fontVariantNumeric: 'tabular-nums', px: 0.5 }}
      >
        {monthHeading(month)}
      </Typography>
      {month.days.map((day) => (
        <HistoryDay key={day.key} day={day} {...actions} />
      ))}
    </Box>
  );
}
