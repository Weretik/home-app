import CheckCircleOutlineOutlinedIcon from '@mui/icons-material/CheckCircleOutlineOutlined';
import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import Typography from '@mui/material/Typography';
import { formatAmount } from '../data/formatting';
import { goalProgress } from './goal-progress';

interface GoalProgressViewProps {
  balance: number;
  /** Above zero. */
  goal: number;
  /** The ring's size in px. */
  size: number;
}

const figure = { fontWeight: 600, fontVariantNumeric: 'tabular-nums' } as const;

/** The percent of the goal as a ring (a tinted track under the primary arc), and now / goal / left beside it. */
export function GoalProgressView({
  balance,
  goal,
  size,
}: GoalProgressViewProps) {
  const { percent, remaining, done } = goalProgress(balance, goal);
  const ring = {
    position: 'absolute',
    inset: 0,
  } as const;
  const rows = [
    { label: 'Зараз', value: formatAmount(balance) },
    { label: 'План', value: formatAmount(goal) },
    ...(done ? [] : [{ label: 'Залишилось', value: formatAmount(remaining) }]),
  ];

  return (
    <Box
      sx={{ display: 'flex', flexWrap: 'wrap', alignItems: 'center', gap: 3 }}
    >
      <Box
        role="img"
        aria-label={`Виконано ${percent}%`}
        sx={{ position: 'relative', width: size, height: size, flexShrink: 0 }}
      >
        <CircularProgress
          variant="determinate"
          value={100}
          size={size}
          thickness={5}
          aria-hidden
          sx={(theme) => ({
            ...ring,
            color: `rgba(${theme.vars.palette.primary.mainChannel} / 0.12)`,
          })}
        />
        <CircularProgress
          variant="determinate"
          value={Math.min(percent, 100)}
          size={size}
          thickness={5}
          aria-hidden
          sx={{
            ...ring,
            color: 'primary.main',
            '& .MuiCircularProgress-circle': { strokeLinecap: 'round' },
          }}
        />
        <Box
          aria-hidden
          sx={{
            ...ring,
            display: 'grid',
            placeContent: 'center',
            textAlign: 'center',
          }}
        >
          <Typography
            variant="h5"
            component="p"
            sx={{ fontVariantNumeric: 'tabular-nums' }}
          >
            {percent}%
          </Typography>
          <Typography variant="caption" color="text.secondary">
            виконано
          </Typography>
        </Box>
      </Box>
      <Box sx={{ display: 'grid', gap: 1.5, minWidth: 0 }}>
        <Box component="dl" sx={{ m: 0, display: 'grid', gap: 1.5 }}>
          {rows.map(({ label, value }) => (
            <Box key={label}>
              <Typography
                component="dt"
                variant="caption"
                color="text.secondary"
              >
                {label}
              </Typography>
              <Typography component="dd" sx={{ ...figure, m: 0 }}>
                {value}
              </Typography>
            </Box>
          ))}
        </Box>
        <Typography variant="body2" color="text.secondary">
          Виконано {percent}%
        </Typography>
        {done && (
          <Typography
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 1,
              fontWeight: 600,
              color: 'primary.main',
            }}
          >
            <CheckCircleOutlineOutlinedIcon fontSize="small" />
            План виконано!
          </Typography>
        )}
      </Box>
    </Box>
  );
}
