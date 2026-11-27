import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Stack from '@mui/material/Stack';
import { useColorScheme } from '@mui/material/styles';
import { useGoogleButton } from './use-google-button';

interface GoogleButtonProps {
  clientId: string;
  /** True while the API checks the credential: a progress indicator takes the button's place (FR-002). */
  busy: boolean;
  /** False after «Вийти»: Google signs nobody in until the button is clicked. */
  autoSelect: boolean;
  onCredential: (credential: string) => void;
}

/** The host of Google's «Sign in with Google» button, its loading state and the retry after a failed load. */
export function GoogleButton({
  clientId,
  busy,
  autoSelect,
  onCredential,
}: GoogleButtonProps) {
  // Google draws the button itself, so it needs the scheme as a value; no styling of ours depends on it.
  const { mode, systemMode } = useColorScheme();
  const scheme =
    (mode === 'system' ? systemMode : mode) === 'dark' ? 'dark' : 'light';
  const { host, status, retry } = useGoogleButton({
    clientId,
    scheme,
    autoSelect,
    onCredential,
  });

  if (status === 'failed')
    return (
      <Stack spacing={1.5} sx={{ alignItems: 'stretch' }}>
        <Alert severity="warning">
          Не вдалося завантажити вхід Google. Перевірте з'єднання.
        </Alert>
        <Button variant="outlined" onClick={retry}>
          Спробувати знову
        </Button>
      </Stack>
    );

  return (
    <Box
      sx={{
        minHeight: 44,
        display: 'grid',
        placeItems: 'center',
        width: '100%',
      }}
    >
      {(busy || status === 'loading') && (
        <CircularProgress size={28} aria-label="Вхід…" />
      )}
      <Box
        ref={host}
        data-testid="google-button"
        sx={{
          display: busy ? 'none' : 'flex',
          justifyContent: 'center',
          width: '100%',
        }}
      />
    </Box>
  );
}
