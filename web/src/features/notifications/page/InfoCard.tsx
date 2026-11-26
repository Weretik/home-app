import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';
import { useId, type ReactNode } from 'react';

interface InfoCardProps {
  title: string;
  caption?: string;
  children: ReactNode;
}

/** A titled card of the page (design.md, "Cards"): an `h2` title, an optional caption, then its content. */
export function InfoCard({ title, caption, children }: InfoCardProps) {
  const titleId = useId();
  return (
    <Card
      component="section"
      aria-labelledby={titleId}
      sx={{ p: { xs: 2, md: 3 }, display: 'grid', gap: 1.5, minWidth: 0 }}
    >
      <Box>
        <Typography
          id={titleId}
          variant="subtitle1"
          component="h2"
          sx={{ fontWeight: 600 }}
        >
          {title}
        </Typography>
        {caption && (
          <Typography variant="body2" color="text.secondary">
            {caption}
          </Typography>
        )}
      </Box>
      {children}
    </Card>
  );
}
