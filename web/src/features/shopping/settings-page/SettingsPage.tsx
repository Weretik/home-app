import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import { DepartmentsCard } from '../department-settings/DepartmentsCard';
import { KeywordsCard } from '../keyword-settings/KeywordsCard';
import { ListSettingsCard } from './ListSettingsCard';
import { useShoppingSettings } from './settings-api';
import { OfflineNotice } from '../../../platform/offline/status/OfflineNotice';
import { useOnline } from '../../../platform/offline/status/use-network-status';

/** «Налаштування» of Покупки (the gear): departments, their keywords, the list's settings (R-11…R-13). */
export function SettingsPage() {
  const online = useOnline();
  const settings = useShoppingSettings();
  return (
    <Box
      sx={{
        display: 'grid',
        gap: { xs: 2, md: 3 },
        gridTemplateColumns: { xs: '1fr', lg: '1fr 1fr' },
        alignItems: 'start',
        minWidth: 0,
      }}
    >
      <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 }, minWidth: 0 }}>
        <DepartmentsCard />
        {settings.isPending && !online && <OfflineNotice compact />}
        {settings.isPending && online && (
          <CircularProgress aria-label="Завантажую налаштування" />
        )}
        {settings.error && (
          <Alert severity="error">
            Не вдалося завантажити налаштування: {settings.error.message}
          </Alert>
        )}
        {settings.data && <ListSettingsCard settings={settings.data} />}
      </Box>
      <KeywordsCard />
    </Box>
  );
}
