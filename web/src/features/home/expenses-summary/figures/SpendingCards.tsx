import Box from '@mui/material/Box';
import type { MonthSummary } from '../data/summary-api';
import { AmountsCard } from './AmountsCard';
import { topCategoryLines, whoSpentLines } from './card-lines';

/** «Хто скільки витратив» and «Топ-5 категорій» side by side from `sm`. */
export function SpendingCards({ summary }: { summary: MonthSummary }) {
  return (
    <Box
      sx={{
        display: 'grid',
        gap: { xs: 2, md: 3 },
        gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' },
      }}
    >
      <AmountsCard
        title="Хто скільки витратив"
        lines={whoSpentLines(summary)}
        empty="Витрат за місяць немає."
      />
      <AmountsCard
        title="Топ-5 категорій"
        lines={topCategoryLines(summary)}
        empty="Витрат за місяць немає."
      />
    </Box>
  );
}
