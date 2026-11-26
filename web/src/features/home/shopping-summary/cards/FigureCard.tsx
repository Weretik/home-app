import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';

interface FigureProps {
  emoji: string;
  title: string;
  value: number;
  caption: string;
}

/** One figure card (design.md, «Figure card»): an emoji and a title, the count, one caption line. */
export function FigureCard({ emoji, title, value, caption }: FigureProps) {
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
        sx={{ my: 0.5, fontVariantNumeric: 'tabular-nums' }}
      >
        {value}
      </Typography>
      <Typography
        variant="body2"
        color="text.secondary"
        sx={{ fontVariantNumeric: 'tabular-nums' }}
      >
        {caption}
      </Typography>
    </Card>
  );
}
