import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { useSignIn } from '../data/identity-api';
import { signInErrorText } from '../data/sign-in-errors';
import { GoogleButton } from './GoogleButton';

interface SignInPageProps {
  /** The public Google OAuth client id; without it sign-in is not set up in this build (FR-004). */
  clientId: string | undefined;
  /** Why the screen shows, such as an expired session (FR-005). */
  notice?: string;
  /** False after «Вийти», so Google does not sign the member straight back in. */
  autoSelect: boolean;
}

/** The sign-in screen at any URL: the brand, the Google button and who may sign in (FR-001…FR-004). */
export function SignInPage({ clientId, notice, autoSelect }: SignInPageProps) {
  const signIn = useSignIn();

  return (
    <Box
      component="main"
      sx={{
        minHeight: '100dvh',
        display: 'grid',
        placeItems: 'center',
        bgcolor: 'background.default',
        px: 2,
        py: 4,
      }}
    >
      <Paper
        variant="outlined"
        sx={{
          width: '100%',
          maxWidth: 400,
          p: { xs: 3, sm: 4 },
          borderRadius: 3,
        }}
      >
        <Stack spacing={3} sx={{ alignItems: 'center', textAlign: 'center' }}>
          <Stack direction="row" spacing={1.25} sx={{ alignItems: 'center' }}>
            <Box
              component="img"
              src="/favicon.svg"
              alt=""
              width={44}
              height={44}
              sx={{ display: 'block' }}
            />
            <Typography
              component="span"
              variant="h6"
              sx={{ textTransform: 'uppercase', letterSpacing: '0.14em' }}
            >
              Дом
            </Typography>
          </Stack>
          <Typography variant="h5" component="h1">
            Вхід до сімейного сайту
          </Typography>
          {notice && !signIn.isError && (
            <Alert severity="info" sx={{ width: '100%', textAlign: 'left' }}>
              {notice}
            </Alert>
          )}
          {signIn.isError && (
            <Alert severity="error" sx={{ width: '100%', textAlign: 'left' }}>
              {signInErrorText(signIn.error)}
            </Alert>
          )}
          {clientId ? (
            <GoogleButton
              clientId={clientId}
              busy={signIn.isPending}
              autoSelect={autoSelect}
              onCredential={(credential) => signIn.mutate(credential)}
            />
          ) : (
            <Alert severity="warning" sx={{ width: '100%', textAlign: 'left' }}>
              Вхід ще не налаштовано
            </Alert>
          )}
          <Typography variant="body2" sx={{ color: 'text.secondary' }}>
            Вхід лише для родини
          </Typography>
        </Stack>
      </Paper>
    </Box>
  );
}
