import LinearProgress from '@mui/material/LinearProgress';
import { fillOf } from './limit-rows';

/** How full a limit is: green under 80 %, amber up to 100 %, red past it. */
export function LimitFill({
  name,
  percent,
}: {
  name: string;
  percent: number;
}) {
  return (
    <LinearProgress
      variant="determinate"
      color={fillOf(percent)}
      value={Math.min(percent, 100)}
      aria-label={`${name}: витрачено ${percent}%`}
      sx={{ height: 6, borderRadius: 3, minWidth: 64 }}
    />
  );
}
