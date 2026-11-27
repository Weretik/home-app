import Alert from '@mui/material/Alert';
import Button from '@mui/material/Button';
import Snackbar from '@mui/material/Snackbar';
import { useState } from 'react';

/** What a list says after a tick: «Готово» with «Скасувати», or why the write failed. */
export interface ResultMessage {
  text: string;
  severity: 'success' | 'error';
  /** «Скасувати» and what it does; none for an error. */
  undo?: () => void;
}

/** The snackbar at the bottom of a list; it hides itself after a few seconds. */
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
  const open = message !== null;

  return (
    <Snackbar
      open={open}
      autoHideDuration={shown?.severity === 'error' ? 8000 : 5000}
      onClose={(_, reason) => reason !== 'clickaway' && onClose()}
      anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      sx={{ bottom: { xs: 88, md: 24 } }}
    >
      {shown ? (
        <Alert
          severity={shown.severity}
          variant="filled"
          onClose={onClose}
          action={
            shown.undo && (
              <Button
                color="inherit"
                size="small"
                onClick={() => {
                  shown.undo?.();
                  onClose();
                }}
              >
                Скасувати
              </Button>
            )
          }
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
