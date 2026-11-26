import { createTheme } from '@mui/material/styles';

export const theme = createTheme({
  palette: { primary: { main: '#2f5bd3' }, background: { default: '#f6f7f9' } },
  shape: { borderRadius: 10 },
  components: {
    MuiButton: { defaultProps: { disableElevation: true } },
    MuiTextField: { defaultProps: { fullWidth: true, size: 'small' } },
  },
});
