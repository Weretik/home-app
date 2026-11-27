import Chip from '@mui/material/Chip';

/** The small «поза лімітами» mark of an expense that counts in the totals but in no limit. */
export function OutsideLimitsChip() {
  return (
    <Chip
      component="span"
      label="поза лімітами"
      size="small"
      variant="outlined"
      title="Враховується в підсумках, але не в лімітах і не переноситься"
      sx={{ ml: 1, height: 20, fontSize: '0.75rem', verticalAlign: 'middle' }}
    />
  );
}
