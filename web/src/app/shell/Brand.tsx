import CastleOutlinedIcon from '@mui/icons-material/CastleOutlined';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { Link } from 'react-router-dom';

/** The logo and the site name in the header; links to the home page. */
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
        sx={(theme) => ({
          width: 38,
          height: 38,
          borderRadius: '12px',
          display: 'grid',
          placeItems: 'center',
          flex: 'none',
          color: 'primary.contrastText',
          background: `linear-gradient(135deg, ${theme.vars.palette.primary.light}, ${theme.vars.palette.primary.dark})`,
        })}
      >
        <CastleOutlinedIcon fontSize="small" />
      </Box>
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
