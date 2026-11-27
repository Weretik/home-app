import Box from '@mui/material/Box';
import LinearProgress from '@mui/material/LinearProgress';
import Typography from '@mui/material/Typography';
import { outsideLimitsLine } from '../data/figure-lines';
import { formatWhole } from '../data/money';
import type { MonthSummary } from '../data/summary-api';
import {
  expensesCaption,
  incomesCaption,
  savingsCaption,
  savingsGoalPercent,
} from './card-lines';
import { KpiCard } from './KpiCard';

/** The four figures at the top of the summary, as in the old «Сводка» sheet. */
export function KpiCards({ summary }: { summary: MonthSummary }) {
  const outsideLimits = outsideLimitsLine(summary);
  const goalPercent = savingsGoalPercent(summary);

  return (
    <Box
      sx={{
        display: 'grid',
        gap: { xs: 2, md: 3 },
        gridTemplateColumns: {
          xs: '1fr',
          sm: 'repeat(2, 1fr)',
          lg: 'repeat(4, 1fr)',
        },
      }}
    >
      <KpiCard
        emoji="💰"
        title="Доходи"
        value={summary.incomes}
        caption={incomesCaption(summary)}
      />
      <KpiCard
        emoji="💸"
        title="Витрати"
        value={summary.expenses}
        caption={expensesCaption(summary)}
      >
        {outsideLimits && (
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{ fontVariantNumeric: 'tabular-nums' }}
          >
            {outsideLimits}
          </Typography>
        )}
      </KpiCard>
      <KpiCard
        emoji="📌"
        title="Підсумок місяця"
        value={summary.result}
        tone={summary.result >= 0 ? 'success' : 'error'}
        caption={`відкладено ${formatWhole(summary.savingsNet)}`}
      />
      <KpiCard
        emoji="🐷"
        title="Скарбничка"
        value={summary.savingsBalance}
        caption={savingsCaption(summary)}
      >
        {goalPercent !== null && (
          <LinearProgress
            variant="determinate"
            value={Math.min(goalPercent, 100)}
            aria-label="Скарбничка: частка від цілі"
            sx={{ mt: 1.5, height: 6, borderRadius: 3 }}
          />
        )}
      </KpiCard>
    </Box>
  );
}
