import Card from '@mui/material/Card';
import type { Theme } from '@mui/material/styles';
import Typography from '@mui/material/Typography';
import useMediaQuery from '@mui/material/useMediaQuery';
import type { MonthSummary } from '../data/summary-api';
import { LimitList } from './LimitList';
import { LimitTable } from './LimitTable';
import { limitRows } from './limit-rows';

/**
 * «Ліміти»: every spending limit of the month, then «РАЗОМ» against the monthly limit; each row notes the overspend
 * carried in, what moves on to the next period and what is planned, when there is any.
 */
export function LimitsCard({ summary }: { summary: MonthSummary }) {
  const isComputer = useMediaQuery((theme: Theme) =>
    theme.breakpoints.up('md'),
  );
  const rows = limitRows(summary);

  return (
    <Card component="section" aria-labelledby="limits-title">
      <Typography
        id="limits-title"
        variant="subtitle1"
        component="h2"
        sx={{ fontWeight: 600, px: { xs: 2, md: 3 }, pt: { xs: 2, md: 2.5 } }}
      >
        Ліміти
      </Typography>
      {summary.limits.length === 0 && (
        <Typography
          variant="body2"
          color="text.secondary"
          sx={{ px: { xs: 2, md: 3 }, pt: 0.5 }}
        >
          Лімітів за категоріями поки немає; нижче весь місяць цілком.
        </Typography>
      )}
      {isComputer ? <LimitTable rows={rows} /> : <LimitList rows={rows} />}
    </Card>
  );
}
