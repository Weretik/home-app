import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';
import { StatusDetails } from './StatusDetails';

/** «Состояние»: whether the API and its dependencies answer. */
export function StatusPage() {
  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 } }}>
      <Typography variant="h4" component="h1">
        Состояние
      </Typography>
      <Card sx={{ p: { xs: 2, md: 3 } }}>
        <StatusDetails />
      </Card>
    </Box>
  );
}
