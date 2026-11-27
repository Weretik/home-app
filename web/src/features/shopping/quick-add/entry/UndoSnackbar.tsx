import Button from '@mui/material/Button';
import Snackbar from '@mui/material/Snackbar';
import { addedMessage, type QuickAddController } from './use-quick-add';

const QUEUED_MESSAGE = 'Додасться, коли з’явиться мережа';

/**
 * «Додано: … · Скасувати» after an add (R-2); «Скасувати» only when the add created new rows. An add made offline says
 * when it will be added instead.
 */
export function UndoSnackbar({ quickAdd }: { quickAdd: QuickAddController }) {
  const { last, queued } = quickAdd;
  const message = last ? addedMessage(last) : QUEUED_MESSAGE;
  return (
    <Snackbar
      open={last !== null || queued}
      autoHideDuration={6000}
      onClose={(_, reason) => reason !== 'clickaway' && quickAdd.dismiss()}
      message={message}
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
