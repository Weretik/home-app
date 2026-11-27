import Alert from '@mui/material/Alert';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import { useId } from 'react';

interface ConfirmDeleteDialogProps {
  title: string;
  /** What is deleted, in one line. */
  summary: string;
  /** What happens, for example «Нотатка видалиться назавжди.». */
  warning: string;
  /** A failed delete's message. */
  error?: string;
  isPending: boolean;
  onConfirm: () => void;
  onClose: () => void;
}

/** Asks before something is deleted for good, with a red «Видалити» (docs/design.md, «Editing entries»). */
export function ConfirmDeleteDialog({
  title,
  summary,
  warning,
  error,
  isPending,
  onConfirm,
  onClose,
}: ConfirmDeleteDialogProps) {
  const titleId = useId();
  return (
    <Dialog
      open
      fullWidth
      maxWidth="xs"
      onClose={isPending ? undefined : onClose}
      aria-labelledby={titleId}
    >
      <DialogTitle id={titleId}>{title}</DialogTitle>
      <DialogContent sx={{ display: 'grid', gap: 2 }}>
        <DialogContentText
          sx={{
            color: 'text.primary',
            fontWeight: 600,
            overflowWrap: 'anywhere',
          }}
        >
          {summary}
        </DialogContentText>
        <DialogContentText>{warning}</DialogContentText>
        {error && <Alert severity="error">{error}</Alert>}
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button onClick={onClose} disabled={isPending}>
          Скасувати
        </Button>
        <Button
          color="error"
          variant="contained"
          onClick={onConfirm}
          disabled={isPending}
        >
          {isPending ? 'Видаляю…' : 'Видалити'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
