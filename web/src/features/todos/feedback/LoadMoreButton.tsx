import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';

interface LoadMoreButtonProps {
  shown: number;
  total: number;
  isLoading: boolean;
  onClick: () => void;
}

/** «Показать ещё» under a list that has more pages; says how many tasks are shown. */
export function LoadMoreButton({
  shown,
  total,
  isLoading,
  onClick,
}: LoadMoreButtonProps) {
  return (
    <Box
      sx={{
        p: 2,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: 2,
        borderTop: 1,
        borderColor: 'divider',
      }}
    >
      <Typography
        variant="body2"
        color="text.secondary"
        sx={{ fontVariantNumeric: 'tabular-nums' }}
      >
        Показано {shown} из {total}
      </Typography>
      <Button variant="outlined" onClick={onClick} disabled={isLoading}>
        {isLoading ? 'Загружаю…' : 'Показать ещё'}
      </Button>
    </Box>
  );
}
