import CloudOffOutlinedIcon from '@mui/icons-material/CloudOffOutlined';
import Alert from '@mui/material/Alert';

/**
 * What a page or card shows in place of its loading state while there is no network and it has no data kept yet
 * (web ADR 0022): it was never opened online on this device. The data comes by itself once the network is back.
 */
export function OfflineNotice({ compact = false }: { compact?: boolean }) {
  return (
    <Alert
      severity="info"
      icon={<CloudOffOutlinedIcon fontSize="inherit" />}
      sx={compact ? undefined : { m: { xs: 2, md: 3 } }}
    >
      Немає звʼязку, а збережених даних тут ще немає. Вони зʼявляться, щойно
      повернеться мережа.
    </Alert>
  );
}
