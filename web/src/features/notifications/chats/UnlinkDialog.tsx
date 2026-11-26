import Alert from '@mui/material/Alert';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import { useId } from 'react';

interface UnlinkDialogProps {
  name: string;
  error?: string;
  isPending: boolean;
  onConfirm: () => void;
  onClose: () => void;
}

/** Asks before a chat is unlinked: the bot stops writing there until `/start` links it again. */
export function UnlinkDialog(props: UnlinkDialogProps) {
  const titleId = useId();
  return (
    <Dialog
      open
      onClose={props.isPending ? undefined : props.onClose}
      aria-labelledby={titleId}
    >
      <DialogTitle id={titleId}>Отвязать чат «{props.name}»?</DialogTitle>
      <DialogContent sx={{ display: 'grid', gap: 2 }}>
        <DialogContentText>
          Бот перестанет туда писать. Подключить снова можно командой /start в
          Телеграме.
        </DialogContentText>
        {props.error && <Alert severity="error">{props.error}</Alert>}
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button onClick={props.onClose} disabled={props.isPending}>
          Отмена
        </Button>
        <Button
          color="error"
          variant="contained"
          onClick={props.onConfirm}
          disabled={props.isPending}
        >
          {props.isPending ? 'Отвязываю…' : 'Отвязать'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
