import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import type { Theme } from '@mui/material/styles';
import useMediaQuery from '@mui/material/useMediaQuery';
import { useId, useState, type FormEvent } from 'react';
import type { NoteItem } from '../data/notes-api';
import { ConfirmDeleteDialog } from '../feedback/ConfirmDeleteDialog';
import {
  describeWriteError,
  isChangedMeanwhile,
  isNoteGone,
} from '../writes/failures-api';
import { useDeleteNote } from '../writes/note-writes-api';
import { NoteFields } from './NoteFields';
import type { NoteDraft } from './note-form';
import { useNoteForm } from './use-note-form';

interface NoteDialogProps {
  /** The note to change as the list holds it now; undefined adds a new one. */
  note?: NoteItem;
  /** What a new note starts with. */
  draft?: NoteDraft;
  onClose: () => void;
  /** The note was deleted elsewhere: the page closes the dialog and says so. */
  onGone: () => void;
}

/**
 * A note in a dialog (FR-005), full screen on a phone and 560 px wide on a computer: the fields, then «Видалити»
 * (asks first), «Скасувати», «Зберегти». Closing waits for a pending save.
 */
export function NoteDialog({ note, draft, onClose, onGone }: NoteDialogProps) {
  const titleId = useId();
  const isPhone = useMediaQuery((theme: Theme) => theme.breakpoints.down('sm'));
  const form = useNoteForm({ current: note, draft, onSaved: onClose, onGone });
  const [confirming, setConfirming] = useState(false);
  const remove = useDeleteNote();
  const busy = form.isPending || remove.isPending;

  const submit = (event: FormEvent) => {
    event.preventDefault();
    form.submit();
  };

  return (
    <Dialog
      open
      fullScreen={isPhone}
      maxWidth={false}
      onClose={busy ? undefined : onClose}
      aria-labelledby={titleId}
      slotProps={{ paper: { sx: { width: '100%', maxWidth: { sm: 560 } } } }}
    >
      <Box
        component="form"
        noValidate
        onSubmit={submit}
        sx={{ display: 'flex', flexDirection: 'column', minHeight: 0, flex: 1 }}
      >
        <DialogTitle id={titleId}>
          {note ? 'Нотатка' : 'Нова нотатка'}
        </DialogTitle>
        <DialogContent
          sx={{
            display: 'grid',
            gap: 2,
            alignContent: 'start',
            '.MuiDialogTitle-root + &': { pt: 1 },
          }}
        >
          {form.message && <Alert severity="error">{form.message}</Alert>}
          <NoteFields
            values={form.values}
            errors={form.errors}
            set={form.set}
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2, gap: 1 }}>
          {note && (
            <Button
              color="error"
              onClick={() => setConfirming(true)}
              disabled={busy}
              sx={{ mr: 'auto' }}
            >
              Видалити
            </Button>
          )}
          <Button onClick={onClose} disabled={form.isPending}>
            Скасувати
          </Button>
          <Button type="submit" variant="contained" disabled={busy}>
            {form.isPending ? 'Зберігаю…' : 'Зберегти'}
          </Button>
        </DialogActions>
      </Box>
      {confirming && note && (
        <ConfirmDeleteDialog
          title="Видалити нотатку?"
          summary={note.title}
          warning="Нотатка видалиться назавжди."
          error={
            remove.error
              ? describeWriteError(remove.error, 'видалити').message
              : undefined
          }
          isPending={remove.isPending}
          onConfirm={() =>
            remove.mutate(
              { id: note.id, version: form.version ?? note.version },
              {
                onSuccess: onClose,
                onError: (error) => {
                  if (isNoteGone(error)) onGone();
                  else if (isChangedMeanwhile(error)) form.reloadWhenNewer();
                },
              },
            )
          }
          onClose={() => {
            remove.reset();
            setConfirming(false);
          }}
        />
      )}
    </Dialog>
  );
}
