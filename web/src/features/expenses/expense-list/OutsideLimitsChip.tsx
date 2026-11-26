import Chip from '@mui/material/Chip';

/** The small «вне лимитов» mark of an expense that counts in the totals but in no limit. */
export function OutsideLimitsChip() {
  return (
    <Chip
      component="span"
      label="вне лимитов"
      size="small"
      variant="outlined"
      title="Считается в итогах, но не в лимитах и не переносится"
      sx={{ ml: 1, height: 20, fontSize: '0.75rem', verticalAlign: 'middle' }}
    />
  );
}
