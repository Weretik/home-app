import ConstructionOutlinedIcon from '@mui/icons-material/ConstructionOutlined';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Card from '@mui/material/Card';
import Typography from '@mui/material/Typography';
import { Link } from 'react-router-dom';

/** A section that is in the menu but not built yet. */
export function ComingSoonPage({ title }: { title: string }) {
  return (
    <Box sx={{ display: 'grid', gap: { xs: 2, md: 3 } }}>
      <Typography variant="h4" component="h1">
        {title}
      </Typography>
      <Card
        sx={{
          p: { xs: 3, md: 5 },
          display: 'grid',
          justifyItems: 'center',
          textAlign: 'center',
          gap: 1.5,
        }}
      >
        <Box
          sx={(theme) => ({
            width: 64,
            height: 64,
            borderRadius: 4,
            display: 'grid',
            placeItems: 'center',
            color: 'primary.main',
            bgcolor: `rgba(${theme.vars.palette.primary.mainChannel} / 0.12)`,
          })}
        >
          <ConstructionOutlinedIcon fontSize="large" />
        </Box>
        <Typography variant="h6" component="p">
          Раздел скоро появится
        </Typography>
        <Typography color="text.secondary" sx={{ maxWidth: 360 }}>
          Пока данные живут в таблицах и ботах. Когда раздел будет готов, он
          заменит их.
        </Typography>
        <Button component={Link} to="/" variant="contained" sx={{ mt: 1 }}>
          На главную
        </Button>
      </Card>
    </Box>
  );
}
