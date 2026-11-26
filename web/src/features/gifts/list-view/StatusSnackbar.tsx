import Alert from '@mui/material/Alert';
import Snackbar from '@mui/material/Snackbar';

/** Why a quick «Подарено» / «Не актуально» / «Вернуть в список» failed; the list has already been refreshed if stale. */
export function StatusSnackbar({
  message,
  onClose,
}: {
  message?: string;
  onClose: () => void;
}) {
  return (
    <Snackbar
      open={Boolean(message)}
      onClose={onClose}
      autoHideDuration={8000}
      anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      sx={{ bottom: { xs: 88, md: 24 } }}
    >
      <Alert
        severity="error"
        onClose={onClose}
        variant="filled"
        sx={{ width: '100%' }}
      >
        {message}
      </Alert>
    </Snackbar>
  );
}
