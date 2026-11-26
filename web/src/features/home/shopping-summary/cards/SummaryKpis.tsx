import Box from '@mui/material/Box';
import type { ShoppingSummary } from '../data/shopping-summary-api';
import { FigureCard } from './FigureCard';

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
      <FigureCard
        emoji="🛒"
        title="Нужно купить"
        value={summary.neededCount}
        caption={`в корзине ${summary.inCartCount}`}
      />
      <FigureCard
        emoji="❗"
        title="Срочно"
        value={summary.urgentCount}
        caption={
          summary.urgentCount > 0 ? 'купить в первую очередь' : 'срочного нет'
        }
      />
      <FigureCard
        emoji="✅"
        title="Куплено за 7 дней"
        value={summary.boughtLast7Days}
        caption={`за 30 дней ${summary.boughtLast30Days}`}
      />
    </Box>
  );
}
