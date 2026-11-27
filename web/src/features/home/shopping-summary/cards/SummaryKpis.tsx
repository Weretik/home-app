import Box from '@mui/material/Box';
import type { ShoppingSummary } from '../data/shopping-summary-api';
import { FigureCard } from './FigureCard';

/** Треба купити, Терміново, Куплено за 7 днів. */
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
        title="Треба купити"
        value={summary.neededCount}
        caption={`у кошику ${summary.inCartCount}`}
      />
      <FigureCard
        emoji="❗"
        title="Терміново"
        value={summary.urgentCount}
        caption={
          summary.urgentCount > 0 ? 'купити насамперед' : 'термінового немає'
        }
      />
      <FigureCard
        emoji="✅"
        title="Куплено за 7 днів"
        value={summary.boughtLast7Days}
        caption={`за 30 днів ${summary.boughtLast30Days}`}
      />
    </Box>
  );
}
