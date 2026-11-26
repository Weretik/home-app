import Box from '@mui/material/Box';
import type { MonthSummary } from '../data/summary-api';
import { AmountsCard } from './AmountsCard';
import { topCategoryLines, whoSpentLines } from './card-lines';

/** «Кто сколько потратил» and «Топ-5 категорий» side by side from `sm`. */
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
        title="Кто сколько потратил"
        lines={whoSpentLines(summary)}
        empty="Расходов за месяц нет."
      />
      <AmountsCard
        title="Топ-5 категорий"
        lines={topCategoryLines(summary)}
        empty="Расходов за месяц нет."
      />
    </Box>
  );
}
