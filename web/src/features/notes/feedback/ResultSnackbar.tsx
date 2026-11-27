import Alert from '@mui/material/Alert';
import Snackbar from '@mui/material/Snackbar';
import { useState } from 'react';

/** What the page says after something happened elsewhere, for example a note deleted meanwhile (FR-005). */
export interface ResultMessage {
  text: string;
  severity: 'success' | 'error' | 'info';
}

/** The snackbar at the bottom of the page; it hides itself after a few seconds. */
export function ResultSnackbar({
  message,
  onClose,
}: {
  message: ResultMessage | null;
  onClose: () => void;
}) {
  // The last message stays while the snackbar slides out.
  const [shown, setShown] = useState(message);
  if (message !== null && message !== shown) setShown(message);

  return (
    <Snackbar
      open={message !== null}
      autoHideDuration={6000}
      onClose={(_, reason) => reason !== 'clickaway' && onClose()}
      anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      sx={{ bottom: { xs: 88, md: 24 } }}
    >
      {shown ? (
        <Alert
          severity={shown.severity}
          variant="filled"
          onClose={onClose}
          sx={{ width: '100%', alignItems: 'center' }}
        >
          {shown.text}
        </Alert>
      ) : (
        <span />
      )}
    </Snackbar>
  );
}
