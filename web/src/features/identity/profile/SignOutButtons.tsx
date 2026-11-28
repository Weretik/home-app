import LogoutIcon from '@mui/icons-material/Logout';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogContentText from '@mui/material/DialogContentText';
import DialogTitle from '@mui/material/DialogTitle';
import { useState } from 'react';
import { useSignOut, useSignOutEverywhere } from '../data/identity-api';

/**
 * «Вийти» (this device) and «Вийти на всіх пристроях» (every device of the member, after a confirmation; ADR 0046),
 * for a lost phone or a token that may have been copied.
 */
export function SignOutButtons() {
  const signOut = useSignOut();
  const everywhere = useSignOutEverywhere();
  const [asking, setAsking] = useState(false);

  return (
    <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
      <Button
        variant="outlined"
        color="inherit"
        startIcon={<LogoutIcon />}
        disabled={signOut.isPending}
        onClick={() => signOut.mutate()}
      >
        Вийти
      </Button>
      <Button color="inherit" onClick={() => setAsking(true)}>
        Вийти на всіх пристроях
      </Button>
      <Dialog
        open={asking}
        onClose={() => setAsking(false)}
        aria-labelledby="sign-out-everywhere-title"
      >
        <DialogTitle id="sign-out-everywhere-title">
          Вийти на всіх пристроях?
        </DialogTitle>
        <DialogContent sx={{ display: 'grid', gap: 2 }}>
          <DialogContentText>
            Вхід закінчиться на всіх ваших телефонах і комп’ютерах, і на цьому
            теж. Увійти знову можна через Google. Корисно, якщо телефон
            загубився.
          </DialogContentText>
          {everywhere.isError && (
            <Alert severity="error">
              Не вдалося вийти на всіх пристроях: {everywhere.error.message}
            </Alert>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAsking(false)}>Скасувати</Button>
          <Button
            color="error"
            variant="contained"
            disabled={everywhere.isPending}
            onClick={() => everywhere.mutate()}
          >
            Вийти всюди
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
