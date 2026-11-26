import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';
import { useId, type ReactNode } from 'react';

interface SummaryCardProps {
  title: string;
  /** A link or button on the right of the title. */
  action?: ReactNode;
  children: ReactNode;
}

/** A titled card of the Gifts summary (a region named by its title). */
export function SummaryCard({ title, action, children }: SummaryCardProps) {
  const titleId = useId();
  return (
    <Card
      component="section"
      aria-labelledby={titleId}
      sx={{
        p: { xs: 2, md: 2.5 },
        display: 'grid',
        gap: 1.5,
        alignContent: 'start',
      }}
    >
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 1,
        }}
      >
        <Typography
          id={titleId}
          variant="subtitle1"
          component="h2"
          sx={{ fontWeight: 600 }}
        >
          {title}
        </Typography>
        {action}
      </Box>
      {children}
    </Card>
  );
}
