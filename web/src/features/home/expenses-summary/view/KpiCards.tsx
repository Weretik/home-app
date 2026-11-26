import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import LinearProgress from '@mui/material/LinearProgress';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';
import { personLabels } from '../data/labels';
import { formatWhole, percentOf } from '../data/money';
import type { MonthSummary } from '../data/summary-api';

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
function KpiCard({
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

/** The four figures at the top of the summary, as in the old «Сводка» sheet. */
export function KpiCards({ summary }: { summary: MonthSummary }) {
  const earned = (person: keyof typeof personLabels) =>
    summary.incomesByPerson.find((i) => i.person === person)?.amount ?? 0;
  const goalPercent = percentOf(summary.savingsBalance, summary.savingsGoal);

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
        title="Доходы"
        value={summary.incomes}
        caption={`Виталий ${formatWhole(earned('Vitaliy'))} · Маша ${formatWhole(earned('Maria'))}`}
      />
      <KpiCard
        emoji="💸"
        title="Расходы"
        value={summary.expenses}
        caption={
          summary.monthlyLimit > 0
            ? `лимит ${formatWhole(summary.monthlyLimit)} · ${percentOf(summary.expenses, summary.monthlyLimit)}%`
            : 'лимит не задан'
        }
      />
      <KpiCard
        emoji="📌"
        title="Итог месяца"
        value={summary.result}
        tone={summary.result >= 0 ? 'success' : 'error'}
        caption={`отложено ${formatWhole(summary.savingsNet)}`}
      />
      <KpiCard
        emoji="🐷"
        title="Копилка"
        value={summary.savingsBalance}
        caption={
          summary.savingsGoal > 0
            ? `${goalPercent}% от цели ${formatWhole(summary.savingsGoal)}`
            : 'цель не задана'
        }
      >
        {summary.savingsGoal > 0 && (
          <LinearProgress
            variant="determinate"
            value={Math.min(goalPercent, 100)}
            aria-label="Копилка: доля от цели"
            sx={{ mt: 1.5, height: 6, borderRadius: 3 }}
          />
        )}
      </KpiCard>
    </Box>
  );
}
