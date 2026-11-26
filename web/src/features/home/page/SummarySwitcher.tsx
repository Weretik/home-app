import Box from '@mui/material/Box';
import ToggleButton from '@mui/material/ToggleButton';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import Typography from '@mui/material/Typography';
import { summarySections } from './summary-sections';

/**
 * Switches the home page between section summaries. Sections without a summary yet stay visible but disabled,
 * marked «скоро». Scrolls sideways on a narrow phone instead of wrapping.
 */
export function SummarySwitcher({
  value,
  onChange,
}: {
  value: string;
  onChange: (key: string) => void;
}) {
  return (
    <Box
      sx={{ overflowX: 'auto', mx: { xs: -2, sm: 0 }, px: { xs: 2, sm: 0 } }}
    >
      <ToggleButtonGroup
        exclusive
        size="small"
        color="primary"
        aria-label="Сводка"
        value={value}
        onChange={(_, next: string | null) => next && onChange(next)}
        sx={{ bgcolor: 'background.paper' }}
      >
        {summarySections.map(({ key, label, icon: Icon, Summary }) => (
          <ToggleButton
            key={key}
            value={key}
            disabled={Summary === null}
            sx={{ gap: 1, px: 2, whiteSpace: 'nowrap', textTransform: 'none' }}
          >
            <Icon fontSize="small" />
            {label}
            {Summary === null && (
              <Typography component="span" variant="caption">
                скоро
              </Typography>
            )}
          </ToggleButton>
        ))}
      </ToggleButtonGroup>
    </Box>
  );
}
