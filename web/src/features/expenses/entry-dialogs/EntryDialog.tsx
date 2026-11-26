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

interface EntryDialogProps {
  title: string;
  /** A failed save's message, shown above the fields. */
  error?: string;
  isPending: boolean;
  onSubmit: () => void;
  onClose: () => void;
  children: ReactNode;
}

/** The add / edit form of an entry in a dialog; full screen on a phone. Closing waits for a pending save. */
export function EntryDialog({
  title,
  error,
  isPending,
  onSubmit,
  onClose,
  children,
}: EntryDialogProps) {
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
            // MUI zeroes the top padding after a title, which clips the first field's label.
            '.MuiDialogTitle-root + &': { pt: 1 },
          }}
        >
          {error && <Alert severity="error">{error}</Alert>}
          {children}
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={onClose} disabled={isPending}>
            Отмена
          </Button>
          <Button type="submit" variant="contained" disabled={isPending}>
            {isPending ? 'Сохраняю…' : 'Сохранить'}
          </Button>
        </DialogActions>
      </Box>
    </Dialog>
  );
}
