import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { useId, type ReactNode } from 'react';

interface TaskGroupProps {
  label: string;
  count: number;
  /** Red heading: the tasks are late. */
  overdue?: boolean;
  children: ReactNode;
}

/** One day group of a list: its heading («Прострочено · 2» in red, «Сьогодні · 3», «Пн, 30 листопада · 1») and its rows. */
export function TaskGroup({ label, count, overdue, children }: TaskGroupProps) {
  const headingId = useId();
  return (
    <Box
      component="section"
      aria-labelledby={headingId}
      sx={{ borderTop: 1, borderColor: 'divider' }}
    >
      <Typography
        id={headingId}
        variant="subtitle2"
        component="h2"
        sx={{
          px: 2,
          pt: 1.5,
          pb: 0.5,
          fontWeight: 600,
          color: overdue ? 'error.main' : 'text.secondary',
        }}
      >
        {label}{' '}
        <Box component="span" sx={{ fontVariantNumeric: 'tabular-nums' }}>
          · {count}
        </Box>
      </Typography>
      {children}
    </Box>
  );
}
