import Button from '@mui/material/Button';
import Snackbar from '@mui/material/Snackbar';
import { useAppUpdate } from './use-app-update';

/** «Доступна нова версія · Оновити» when a new deploy waits (FR-002); it stays until tapped or closed. */
export function UpdatePrompt() {
  const { needRefresh, update, dismiss } = useAppUpdate();

  return (
    <Snackbar
      open={needRefresh}
      onClose={(_, reason) => reason !== 'clickaway' && dismiss()}
      message="Доступна нова версія"
      anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      sx={{
        bottom: { xs: 'calc(88px + env(safe-area-inset-bottom, 0px))', md: 24 },
      }}
      action={
        <Button color="inherit" size="small" onClick={update}>
          Оновити
        </Button>
      }
    />
  );
}
