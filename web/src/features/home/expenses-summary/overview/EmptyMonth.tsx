import EventNoteOutlinedIcon from '@mui/icons-material/EventNoteOutlined';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';

/** A month with no entries and no plans: what will show here, and how to open another month. */
export function EmptyMonth() {
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
        <EventNoteOutlinedIcon />
      </Box>
      <Typography variant="h6" component="p">
        За этот месяц записей нет
      </Typography>
      <Typography color="text.secondary" sx={{ maxWidth: 360 }}>
        Лимиты, кто сколько потратил и главные категории появятся, когда в
        месяце будут доходы или расходы. Стрелками выше можно открыть другой
        месяц.
      </Typography>
    </Card>
  );
}
