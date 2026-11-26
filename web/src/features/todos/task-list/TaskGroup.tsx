import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { useId, type ReactNode } from 'react';
import { dueBucketLabels, type DueBucket } from '../data/due-date';

interface TaskGroupProps {
  bucket: DueBucket;
  count: number;
  children: ReactNode;
}

/** One due group of the active list: its heading («Просрочено · 2», red when overdue) and its rows. */
export function TaskGroup({ bucket, count, children }: TaskGroupProps) {
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
          color: bucket === 'overdue' ? 'error.main' : 'text.secondary',
        }}
      >
        {dueBucketLabels[bucket]}{' '}
        <Box component="span" sx={{ fontVariantNumeric: 'tabular-nums' }}>
          · {count}
        </Box>
      </Typography>
      {children}
    </Box>
  );
}
