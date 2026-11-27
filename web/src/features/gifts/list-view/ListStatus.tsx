import CardGiftcardOutlinedIcon from '@mui/icons-material/CardGiftcardOutlined';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import CircularProgress from '@mui/material/CircularProgress';
import Typography from '@mui/material/Typography';
import type { ReactNode } from 'react';

interface ListStatusProps {
  isPending: boolean;
  error: Error | null;
  isEmpty: boolean;
  isFiltered: boolean;
  loadingLabel: string;
  empty: { title: string; hint: string };
  onRetry: () => void;
  onReset: () => void;
  children: ReactNode;
}

/** Loading, error and empty states of a gift tab (docs/design.md: designed, not blank); the list otherwise. */
export function ListStatus(props: ListStatusProps) {
  const { isPending, error, isEmpty, isFiltered, empty, children } = props;
  if (isPending)
    return (
      <Box sx={{ display: 'grid', placeItems: 'center', py: 6 }}>
        <CircularProgress aria-label={props.loadingLabel} />
      </Box>
    );
  if (error)
    return (
      <Alert
        severity="error"
        action={
          <Button color="inherit" size="small" onClick={props.onRetry}>
            Повторити
          </Button>
        }
      >
        Не вдалося завантажити: {error.message}
      </Alert>
    );
  if (!isEmpty) return children;

  return (
    <Card
      sx={{
        p: { xs: 3, md: 5 },
        display: 'grid',
        justifyItems: 'center',
        textAlign: 'center',
        gap: 1,
      }}
    >
      <Box
        sx={(theme) => ({
          width: 56,
          height: 56,
          borderRadius: 3,
          display: 'grid',
          placeItems: 'center',
          color: 'primary.main',
          bgcolor: `rgba(${theme.vars.palette.primary.mainChannel} / 0.12)`,
        })}
      >
        <CardGiftcardOutlinedIcon />
      </Box>
      <Typography variant="h6" component="p">
        {isFiltered ? 'З цими фільтрами подарунків немає' : empty.title}
      </Typography>
      <Typography color="text.secondary" sx={{ maxWidth: 360 }}>
        {isFiltered
          ? 'Скиньте фільтри, щоб побачити всі подарунки.'
          : empty.hint}
      </Typography>
      {isFiltered && (
        <Button onClick={props.onReset} sx={{ mt: 1 }}>
          Скинути фільтри
        </Button>
      )}
    </Card>
  );
}
