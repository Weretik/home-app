import Box from '@mui/material/Box';
import { useBudget } from '../budget-data/budget-api';
import { useSavingsMovements, useSavingsOverview } from '../data/savings-api';
import { MonthMovements } from '../savings-movements/MonthMovements';
import { GoalCard } from '../savings-goal/GoalCard';
import { useMonthParam } from '../url-state/use-month-param';
import { BalanceCard } from './BalanceCard';
import { SavingsMonthsCard } from './SavingsMonthsCard';

const twoColumns = {
  display: 'grid',
  gap: { xs: 2, md: 3 },
  gridTemplateColumns: {
    xs: 'minmax(0, 1fr)',
    lg: 'minmax(0, 2fr) minmax(0, 1fr)',
  },
} as const;

/** «Копилка»: what is in the jar now and how far to its goal, the open month's movements, and every month's sums. */
export function SavingsPage() {
  const [month, setMonth] = useMonthParam();
  const overview = useSavingsOverview();
  const budget = useBudget();
  // The same query as the movements list below (React Query shares it): the open month's figures for the card.
  const openMonth = useSavingsMovements(month).data?.pages[0];
  const currentBalance =
    overview.data?.currentBalance ?? openMonth?.currentBalance;

  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
      <Box sx={twoColumns}>
        <BalanceCard currentBalance={currentBalance} month={openMonth} />
        {!budget.error && (
          <GoalCard balance={currentBalance} goal={budget.data?.savingsGoal} />
        )}
      </Box>
      <Box sx={{ ...twoColumns, alignItems: 'start' }}>
        <MonthMovements month={month} />
        <SavingsMonthsCard
          months={overview.data?.months}
          error={overview.error}
          open={month}
          onOpen={setMonth}
          onRetry={() => void overview.refetch()}
        />
      </Box>
    </Box>
  );
}
