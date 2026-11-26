import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';
import { formatWhole } from '../data/money';

interface KpiCardProps {
  emoji: string;
  title: string;
  value: number;
  /** A status colour for a value that is good or bad (the month's result); plain text otherwise. */
  tone?: 'success' | 'error';
  caption: string;
  children?: ReactNode;
}

/** One figure of the month: an emoji and a title, the rounded amount, one line under it. */
export function KpiCard({
  emoji,
  title,
  value,
  tone,
  caption,
  children,
}: KpiCardProps) {
  return (
    <Card component="section" aria-label={title} sx={{ p: 2.5 }}>
      <Typography
        variant="subtitle1"
        component="h2"
        sx={{ fontWeight: 600, color: 'text.secondary' }}
      >
        <Box component="span" aria-hidden sx={{ mr: 1 }}>
          {emoji}
        </Box>
        {title}
      </Typography>
      <Typography
        variant="h4"
        component="p"
        sx={{
          my: 0.5,
          fontVariantNumeric: 'tabular-nums',
          color: tone ? `${tone}.main` : 'text.primary',
        }}
      >
        {formatWhole(value)}
      </Typography>
      <Typography
        variant="body2"
        color="text.secondary"
        sx={{ fontVariantNumeric: 'tabular-nums' }}
      >
        {caption}
      </Typography>
      {children}
    </Card>
  );
}
