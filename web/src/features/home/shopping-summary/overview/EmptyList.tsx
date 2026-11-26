import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';

/** Shown instead of the lists while nothing is needed. */
export function EmptyList() {
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
      <Typography variant="h6" component="p">
        Список пуст 🎉
      </Typography>
      <Typography color="text.secondary" sx={{ maxWidth: 360 }}>
        Всё куплено. Новое добавляется в разделе «Покупки».
      </Typography>
    </Card>
  );
}
