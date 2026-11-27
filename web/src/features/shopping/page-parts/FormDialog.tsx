import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import type { Theme } from '@mui/material/styles';
import useMediaQuery from '@mui/material/useMediaQuery';
import { useId, type FormEvent, type ReactNode } from 'react';

interface FormDialogProps {
  title: string;
  /** A failed save's message, shown above the fields. */
  error?: string;
  isPending: boolean;
  onSubmit: () => void;
  onClose: () => void;
  /** Extra buttons on the left of the actions («Видалити»). */
  extraAction?: ReactNode;
  children: ReactNode;
}

/** An add / edit form in a dialog (design.md, «Editing entries»); full screen on a phone. */
export function FormDialog({
  title,
  error,
  isPending,
  onSubmit,
  onClose,
  extraAction,
  children,
}: FormDialogProps) {
  const titleId = useId();
  const isPhone = useMediaQuery((theme: Theme) => theme.breakpoints.down('sm'));
  const submit = (event: FormEvent) => {
    event.preventDefault();
    onSubmit();
  };

  return (
    <Dialog
      open
      fullWidth
      maxWidth="xs"
      fullScreen={isPhone}
      onClose={isPending ? undefined : onClose}
      aria-labelledby={titleId}
    >
      <Box
        component="form"
        noValidate
        onSubmit={submit}
        sx={{ display: 'flex', flexDirection: 'column', minHeight: 0, flex: 1 }}
      >
        <DialogTitle id={titleId}>{title}</DialogTitle>
        <DialogContent
          sx={{
            display: 'grid',
            gap: 2,
            alignContent: 'start',
            '.MuiDialogTitle-root + &': { pt: 1 },
          }}
        >
          {error && <Alert severity="error">{error}</Alert>}
          {children}
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          {extraAction && <Box sx={{ mr: 'auto' }}>{extraAction}</Box>}
          <Button onClick={onClose} disabled={isPending}>
            Скасувати
          </Button>
          <Button type="submit" variant="contained" disabled={isPending}>
            {isPending ? 'Зберігаю…' : 'Зберегти'}
          </Button>
        </DialogActions>
      </Box>
    </Dialog>
  );
}
