import AddOutlinedIcon from '@mui/icons-material/AddOutlined';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import CircularProgress from '@mui/material/CircularProgress';
import Typography from '@mui/material/Typography';
import { useId, type ReactNode } from 'react';
import { OfflineNotice } from '../../../platform/offline/status/OfflineNotice';
import { useOnline } from '../../../platform/offline/status/use-network-status';

interface SettingsCardProps {
  title: string;
  caption: string;
  addLabel: string;
  onAdd: () => void;
  isPending: boolean;
  error: Error | null;
  onRetry: () => void;
  children: ReactNode;
}

/** A settings card: title, caption and its add button, then loading, a failed load with «Повторити», or the list. */
export function SettingsCard(props: SettingsCardProps) {
  const online = useOnline();
  const titleId = useId();

  return (
    <Card component="section" aria-labelledby={titleId}>
      <Box
        sx={{
          p: { xs: 2, md: 3 },
          pb: { xs: 1, md: 1 },
          display: 'flex',
          flexWrap: 'wrap',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: 2,
        }}
      >
        <Box>
          <Typography
            id={titleId}
            variant="subtitle1"
            component="h2"
            sx={{ fontWeight: 600 }}
          >
            {props.title}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {props.caption}
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddOutlinedIcon />}
          onClick={props.onAdd}
        >
          {props.addLabel}
        </Button>
      </Box>
      <Box sx={{ px: { xs: 2, md: 3 }, pb: 2 }}>
        {props.isPending && !online ? (
          <OfflineNotice compact />
        ) : props.isPending ? (
          <Box sx={{ display: 'grid', placeItems: 'center', py: 4 }}>
            <CircularProgress aria-label={`Завантажую: ${props.title}`} />
          </Box>
        ) : props.error ? (
          <Alert
            severity="error"
            action={
              <Button color="inherit" size="small" onClick={props.onRetry}>
                Повторити
              </Button>
            }
          >
            Не вдалося завантажити: {props.error.message}
          </Alert>
        ) : (
          props.children
        )}
      </Box>
    </Card>
  );
}
