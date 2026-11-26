import SavingsOutlinedIcon from '@mui/icons-material/SavingsOutlined';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Skeleton from '@mui/material/Skeleton';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';
import { formatAmount } from '../data/formatting';
import { SignedAmount } from './SignedAmount';

interface MonthFigures {
  balanceAtStart: number;
  deposits: number;
  withdrawals: number;
  balanceAtEnd: number;
}

interface BalanceCardProps {
  /** Undefined while loading. */
  currentBalance?: number;
  /** The open month's sums; undefined while loading. */
  month?: MonthFigures;
}

const plain = { fontWeight: 600, fontVariantNumeric: 'tabular-nums' } as const;

/** What is in the jar now, big, and the open month's movement under it. */
export function BalanceCard({ currentBalance, month }: BalanceCardProps) {
  const figures: { label: string; value?: ReactNode }[] = [
    {
      label: 'На начало месяца',
      value: month && (
        <Typography sx={plain}>{formatAmount(month.balanceAtStart)}</Typography>
      ),
    },
    {
      label: 'Положили',
      value: month && <SignedAmount value={month.deposits} sx={plain} />,
    },
    {
      label: 'Взяли',
      value: month && <SignedAmount value={-month.withdrawals} sx={plain} />,
    },
    {
      label: 'На конец месяца',
      value: month && (
        <Typography sx={plain}>{formatAmount(month.balanceAtEnd)}</Typography>
      ),
    },
  ];

  return (
    <Card
      component="section"
      aria-label="Копилка"
      sx={{ p: { xs: 2, md: 3 }, display: 'grid', gap: { xs: 2, md: 3 } }}
    >
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
        <Box
          sx={(theme) => ({
            width: 48,
            height: 48,
            flexShrink: 0,
            borderRadius: 3,
            display: 'grid',
            placeItems: 'center',
            color: 'primary.main',
            bgcolor: `rgba(${theme.vars.palette.primary.mainChannel} / 0.12)`,
          })}
        >
          <SavingsOutlinedIcon />
        </Box>
        <Box sx={{ minWidth: 0 }}>
          <Typography variant="body2" color="text.secondary">
            В копилке сейчас
          </Typography>
          {currentBalance === undefined ? (
            <Skeleton width={200} height={48} />
          ) : (
            <Typography
              variant="h4"
              component="p"
              sx={{ fontVariantNumeric: 'tabular-nums' }}
            >
              {formatAmount(currentBalance)}
            </Typography>
          )}
        </Box>
      </Box>
      <Box
        component="dl"
        sx={{
          m: 0,
          display: 'grid',
          gap: 2,
          gridTemplateColumns: { xs: '1fr 1fr', md: 'repeat(4, auto)' },
          justifyContent: { md: 'start' },
          columnGap: { md: 5 },
        }}
      >
        {figures.map(({ label, value }) => (
          <Box key={label}>
            <Typography component="dt" variant="caption" color="text.secondary">
              {label}
            </Typography>
            <Box component="dd" sx={{ m: 0 }}>
              {value ?? <Skeleton width={100} />}
            </Box>
          </Box>
        ))}
      </Box>
    </Card>
  );
}
