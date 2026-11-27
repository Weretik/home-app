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

/**
 * What is in the jar now, big, and the open month's movement as four tiles: under it on phones, beside it on wide
 * screens so the card has no empty half.
 */
export function BalanceCard({ currentBalance, month }: BalanceCardProps) {
  const figures: { label: string; value?: ReactNode }[] = [
    {
      label: 'На початок місяця',
      value: month && (
        <Typography sx={plain}>{formatAmount(month.balanceAtStart)}</Typography>
      ),
    },
    {
      label: 'Поклали',
      value: month && <SignedAmount value={month.deposits} sx={plain} />,
    },
    {
      label: 'Взяли',
      value: month && <SignedAmount value={-month.withdrawals} sx={plain} />,
    },
    {
      label: 'На кінець місяця',
      value: month && (
        <Typography sx={plain}>{formatAmount(month.balanceAtEnd)}</Typography>
      ),
    },
  ];

  return (
    <Card
      component="section"
      aria-label="Скарбничка"
      sx={{
        p: { xs: 2, md: 3 },
        display: 'grid',
        gap: { xs: 2, md: 3 },
        alignItems: 'center',
        gridTemplateColumns: {
          xs: 'minmax(0, 1fr)',
          md: 'minmax(0, 1fr) minmax(0, 1.4fr)',
        },
      }}
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
            У скарбничці зараз
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
          gap: { xs: 1, md: 1.5 },
          gridTemplateColumns: 'repeat(2, minmax(0, 1fr))',
        }}
      >
        {figures.map(({ label, value }) => (
          <Box
            key={label}
            sx={{ px: 1.5, py: 1.25, borderRadius: 2, bgcolor: 'action.hover' }}
          >
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
