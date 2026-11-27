import Button from '@mui/material/Button';

/** «Повторити» in a page's error or offline alert. */
export function RetryButton({ onRetry }: { onRetry: () => void }) {
  return (
    <Button color="inherit" size="small" onClick={onRetry}>
      Повторити
    </Button>
  );
}
