import Alert from '@mui/material/Alert';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import { useId } from 'react';

interface DeleteDialogProps {
  /** «Видалити подарунок?» */
  title: string;
  /** The item in one line: «Сережки · Маша». */
  summary: string;
  /** Why the delete failed, in Russian. */
  error?: string;
  isPending: boolean;
  onConfirm: () => void;
  onClose: () => void;
}

/** Asks before an item is deleted for good (docs/design.md, «Editing entries»). */
export function DeleteDialog({
  title,
  summary,
  error,
  isPending,
  onConfirm,
  onClose,
}: DeleteDialogProps) {
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
        <DialogContentText>Видалиться назавжди.</DialogContentText>
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
