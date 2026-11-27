import Button from '@mui/material/Button';
import Snackbar from '@mui/material/Snackbar';
import { addedMessage, type QuickAddController } from './use-quick-add';

/** «Додано: … · Скасувати» after an add (R-2); «Скасувати» only when the add created new rows. */
export function UndoSnackbar({ quickAdd }: { quickAdd: QuickAddController }) {
  const { last } = quickAdd;
  return (
    <Snackbar
      open={last !== null}
      autoHideDuration={6000}
      onClose={(_, reason) => reason !== 'clickaway' && quickAdd.dismiss()}
      message={last ? addedMessage(last) : ''}
      anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      sx={{
        bottom: { xs: 'calc(88px + env(safe-area-inset-bottom, 0px))', md: 24 },
      }}
      action={
        last && last.added.length > 0 ? (
          <Button color="inherit" size="small" onClick={quickAdd.undoLast}>
            Скасувати
          </Button>
        ) : undefined
      }
    />
  );
}
