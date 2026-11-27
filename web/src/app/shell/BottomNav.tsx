import BottomNavigation from '@mui/material/BottomNavigation';
import BottomNavigationAction from '@mui/material/BottomNavigationAction';
import Paper from '@mui/material/Paper';
import { NavLink, useLocation } from 'react-router-dom';
import { mainSections, sectionOf } from './sections';

/** The phone's bottom bar with the five main sections; hidden on a computer. */
export function BottomNav() {
  const { pathname } = useLocation();
  const current = sectionOf(pathname)?.path ?? false;

  return (
    <Paper
      component="nav"
      aria-label="Розділи"
      square
      sx={(theme) => ({
        display: { xs: 'block', md: 'none' },
        position: 'fixed',
        left: 0,
        right: 0,
        bottom: 0,
        zIndex: theme.zIndex.appBar,
        borderTop: 1,
        borderColor: 'divider',
        pb: 'env(safe-area-inset-bottom, 0px)',
        bgcolor: `rgba(${theme.vars.palette.background.paperChannel} / 0.92)`,
        backdropFilter: 'blur(12px)',
      })}
    >
      <BottomNavigation
        value={current}
        showLabels
        sx={{ bgcolor: 'transparent', height: 64 }}
      >
        {mainSections.map(({ path, label, icon: Icon }) => (
          <BottomNavigationAction
            key={path}
            value={path}
            label={label}
            icon={<Icon />}
            component={NavLink}
            to={path}
            end={path === '/'}
            sx={(theme) => ({
              minWidth: 0,
              px: 0.5,
              color: 'text.secondary',
              '& .MuiBottomNavigationAction-label': { fontSize: 11, mt: 0.5 },
              '& .MuiBottomNavigationAction-label.Mui-selected': {
                fontSize: 11,
                fontWeight: 600,
              },
              '& svg': {
                width: 48,
                height: 28,
                px: '12px',
                borderRadius: 14,
                transition: 'background-color 150ms',
              },
              '&.Mui-selected svg': {
                bgcolor: `rgba(${theme.vars.palette.primary.mainChannel} / 0.14)`,
              },
            })}
          />
        ))}
      </BottomNavigation>
    </Paper>
  );
}
