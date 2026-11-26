import Alert from '@mui/material/Alert';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import type { UseMutationResult } from '@tanstack/react-query';
import { useId } from 'react';
import { describeWriteError } from '../write-data/write-errors-api';

interface DeleteEntryDialogProps {
  title: string;
  /** The entry in one line: «Продукты · 1 234,50 ₴ · чт, 1 окт.». */
  summary: string;
  id: string;
  /** The delete mutation of the entry's kind. */
  remove: UseMutationResult<unknown, Error, string>;
  onClose: () => void;
}

/** Asks before an entry is deleted for good. */
export function DeleteEntryDialog({
  title,
  summary,
  id,
  remove,
  onClose,
}: DeleteEntryDialogProps) {
  const titleId = useId();
  // One delete mutation serves every row of a list, so a failure is forgotten when its dialog closes.
  const close = () => {
    remove.reset();
    onClose();
  };
  const error = remove.error
    ? describeWriteError(remove.error, 'удалить').message
    : undefined;

  return (
    <Dialog
      open
      fullWidth
      maxWidth="xs"
      onClose={remove.isPending ? undefined : close}
      aria-labelledby={titleId}
    >
      <DialogTitle id={titleId}>{title}</DialogTitle>
      <DialogContent sx={{ display: 'grid', gap: 2 }}>
        <DialogContentText sx={{ color: 'text.primary', fontWeight: 600 }}>
          {summary}
        </DialogContentText>
        <DialogContentText>Запись удалится насовсем.</DialogContentText>
        {error && <Alert severity="error">{error}</Alert>}
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button onClick={close} disabled={remove.isPending}>
          Отмена
        </Button>
        <Button
          color="error"
          variant="contained"
          onClick={() => remove.mutate(id, { onSuccess: close })}
          disabled={remove.isPending}
        >
          {remove.isPending ? 'Удаляю…' : 'Удалить'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
