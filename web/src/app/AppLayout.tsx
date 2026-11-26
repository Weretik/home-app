import AppBar from '@mui/material/AppBar';
import Button from '@mui/material/Button';
import Container from '@mui/material/Container';
import Toolbar from '@mui/material/Toolbar';
import Typography from '@mui/material/Typography';
import { NavLink, Outlet } from 'react-router-dom';

/** Each feature adds its section here when it gets a page. */
const sections = [{ path: '/status', label: 'Состояние' }];

export function AppLayout() {
  return (
    <>
      <AppBar
        position="static"
        color="inherit"
        elevation={0}
        sx={{ borderBottom: 1, borderColor: 'divider' }}
      >
        <Toolbar sx={{ gap: 1 }}>
          <Typography variant="h6" sx={{ mr: 2 }}>
            Дом
          </Typography>
          {sections.map((section) => (
            <Button
              key={section.path}
              component={NavLink}
              to={section.path}
              sx={{ '&.active': { bgcolor: 'action.selected' } }}
            >
              {section.label}
            </Button>
          ))}
        </Toolbar>
      </AppBar>
      <Container maxWidth="md" sx={{ py: 3 }}>
        <Outlet />
      </Container>
    </>
  );
}
