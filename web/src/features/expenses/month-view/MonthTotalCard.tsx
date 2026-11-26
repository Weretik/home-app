import type SvgIcon from '@mui/material/SvgIcon';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Skeleton from '@mui/material/Skeleton';
import Typography from '@mui/material/Typography';
import { formatAmount, formatRecordCount } from '../data/formatting';

interface MonthTotalCardProps {
  title: string;
  icon: typeof SvgIcon;
  color: 'primary' | 'secondary';
  /** Undefined while the month is loading. */
  total?: { amount: number; count: number };
}

/** The month's sum for the current filter, with the number of records. */
export function MonthTotalCard({
  title,
  icon: Icon,
  color,
  total,
}: MonthTotalCardProps) {
  return (
    <Card
      component="section"
      aria-label={title}
      sx={{
        p: { xs: 2, md: 3 },
        display: 'flex',
        alignItems: 'center',
        gap: 2,
      }}
    >
      <Box
        sx={(theme) => ({
          width: 48,
          height: 48,
          flexShrink: 0,
          borderRadius: 3,
          display: 'grid',
          placeItems: 'center',
          color: `${color}.main`,
          bgcolor: `rgba(${theme.vars.palette[color].mainChannel} / 0.12)`,
        })}
      >
        <Icon />
      </Box>
      <Box sx={{ minWidth: 0 }}>
        <Typography variant="body2" color="text.secondary">
          {title}
        </Typography>
        {total ? (
          <>
            <Typography
              variant="h5"
              component="p"
              sx={{ fontVariantNumeric: 'tabular-nums' }}
            >
              {formatAmount(total.amount)}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {formatRecordCount(total.count)}
            </Typography>
          </>
        ) : (
          <Skeleton width={160} height={40} />
        )}
      </Box>
    </Card>
  );
}
