import FlagOutlinedIcon from '@mui/icons-material/FlagOutlined';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Link from '@mui/material/Link';
import Skeleton from '@mui/material/Skeleton';
import Typography from '@mui/material/Typography';
import { Link as RouterLink } from 'react-router-dom';
import { settingsPath } from '../section/expenses-pages';
import { GoalProgressView } from './GoalProgressView';

interface GoalCardProps {
  /** What is in the jar now; undefined while loading. */
  balance?: number;
  /** The savings goal (0 = none); undefined while loading. */
  goal?: number;
}

const RING = 132;

/** «План скарбнички»: a ring of the percent done, with now, the goal and what is left. No goal: a link to set one. */
export function GoalCard({ balance, goal }: GoalCardProps) {
  const loading = balance === undefined || goal === undefined;

  return (
    <Card
      component="section"
      aria-labelledby="savings-goal-title"
      sx={{
        p: { xs: 2, md: 3 },
        display: 'grid',
        gap: 2,
        alignContent: 'start',
      }}
    >
      <Typography
        id="savings-goal-title"
        variant="subtitle1"
        component="h2"
        sx={{ fontWeight: 600 }}
      >
        План скарбнички
      </Typography>
      {loading ? (
        <Box sx={{ display: 'flex', gap: 3, alignItems: 'center' }}>
          <Skeleton variant="circular" width={RING} height={RING} />
          <Box sx={{ flex: 1 }}>
            <Skeleton />
            <Skeleton />
            <Skeleton />
          </Box>
        </Box>
      ) : goal > 0 ? (
        <GoalProgressView balance={balance} goal={goal} size={RING} />
      ) : (
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'flex-start' }}>
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
            <FlagOutlinedIcon />
          </Box>
          <Box>
            <Typography>План не задано.</Typography>
            <Typography variant="body2" color="text.secondary">
              Задайте, скільки хочете накопичити, і тут з’явиться, скільки вже
              зроблено.{' '}
              <Link component={RouterLink} to={settingsPath}>
                Задати план у налаштуваннях
              </Link>
            </Typography>
          </Box>
        </Box>
      )}
    </Card>
  );
}
