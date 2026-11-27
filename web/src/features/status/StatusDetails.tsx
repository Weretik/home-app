import Alert from '@mui/material/Alert';
import CircularProgress from '@mui/material/CircularProgress';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Typography from '@mui/material/Typography';
import { useSystemStatus } from './status-api';
import { OfflineNotice } from '../../platform/offline/status/OfflineNotice';
import { useOnline } from '../../platform/offline/status/use-network-status';

const checkNames: Record<string, string> = { database: 'База даних' };

/** The API's answer: its status, environment and version, and each dependency's check; loading and error states. */
export function StatusDetails() {
  const online = useOnline();
  const { data, error, isPending } = useSystemStatus();

  if (isPending && !online) return <OfflineNotice compact />;
  if (isPending) return <CircularProgress aria-label="Перевіряю API" />;
  if (error)
    return <Alert severity="error">API недоступний: {error.message}</Alert>;

  const checks = Object.entries(data.checks);
  return (
    <section aria-label="Стан API">
      <Alert severity={data.status === 'Healthy' ? 'success' : 'warning'}>
        API: <strong>{data.status}</strong>
      </Alert>
      <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
        {data.environment}, версія {data.version}
      </Typography>
      {checks.length > 0 && (
        <List dense>
          {checks.map(([name, status]) => (
            <ListItem key={name} disableGutters>
              <ListItemText
                primary={checkNames[name] ?? name}
                secondary={status}
              />
            </ListItem>
          ))}
        </List>
      )}
    </section>
  );
}
