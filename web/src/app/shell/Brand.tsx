import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { Link } from 'react-router-dom';

/** The logo (the same house as the favicon) and the site name in the header; links to the home page. */
export function Brand() {
  return (
    <Box
      component={Link}
      to="/"
      aria-label="Дом, на главную"
      sx={{
        display: 'inline-flex',
        alignItems: 'center',
        gap: 1.25,
        color: 'text.primary',
        textDecoration: 'none',
        borderRadius: 2,
      }}
    >
      <Box
        component="img"
        src="/favicon.svg"
        alt=""
        width={38}
        height={38}
        sx={{ display: 'block', flex: 'none' }}
      />
      <Typography
        component="span"
        variant="h6"
        sx={{ textTransform: 'uppercase', letterSpacing: '0.14em' }}
      >
        Дом
      </Typography>
    </Box>
  );
}
