import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';
import type { ShoppingSummary } from '../data/shopping-summary-api';

interface FigureProps {
  emoji: string;
  title: string;
  value: number;
  caption: string;
}

/** One figure card (design.md, «Figure card»): an emoji and a title, the count, one caption line. */
function Figure({ emoji, title, value, caption }: FigureProps) {
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

/** Нужно купить, Срочно, Куплено за 7 дней. */
export function SummaryKpis({ summary }: { summary: ShoppingSummary }) {
  return (
    <Box
      sx={{
        display: 'grid',
        gap: { xs: 2, md: 3 },
        gridTemplateColumns: {
          xs: '1fr',
          sm: 'repeat(2, 1fr)',
          lg: 'repeat(3, 1fr)',
        },
      }}
    >
      <Figure
        emoji="🛒"
        title="Нужно купить"
        value={summary.neededCount}
        caption={`в корзине ${summary.inCartCount}`}
      />
      <Figure
        emoji="❗"
        title="Срочно"
        value={summary.urgentCount}
        caption={
          summary.urgentCount > 0 ? 'купить в первую очередь' : 'срочного нет'
        }
      />
      <Figure
        emoji="✅"
        title="Куплено за 7 дней"
        value={summary.boughtLast7Days}
        caption={`за 30 дней ${summary.boughtLast30Days}`}
      />
    </Box>
  );
}
