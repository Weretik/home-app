import BottomNavigation from '@mui/material/BottomNavigation';
import BottomNavigationAction from '@mui/material/BottomNavigationAction';
import MoreHorizIcon from '@mui/icons-material/MoreHoriz';
import Paper from '@mui/material/Paper';
import type { Theme } from '@mui/material/styles';
import { useState } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { MoreSheet } from './MoreSheet';
import { mainSections, phoneMoreSections, sectionOf } from './sections';

const MORE = 'more';

const actionSx = (theme: Theme) => ({
  minWidth: 0,
  px: 0.25,
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
});

/**
 * The phone's bottom bar: the five main sections and «Ще», which opens a sheet with the rest (web ADR 0019);
 * hidden on a computer.
 */
export function BottomNav() {
  const { pathname } = useLocation();
  const [moreOpen, setMoreOpen] = useState(false);
  const section = sectionOf(pathname);
  const inMore = section !== undefined && phoneMoreSections.includes(section);
  const current = inMore ? MORE : (section?.path ?? false);

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
            sx={actionSx}
          />
        ))}
        <BottomNavigationAction
          value={MORE}
          label="Ще"
          icon={<MoreHorizIcon />}
          aria-haspopup="dialog"
          aria-expanded={moreOpen}
          onClick={() => setMoreOpen(true)}
          sx={actionSx}
        />
      </BottomNavigation>
      <MoreSheet open={moreOpen} onClose={() => setMoreOpen(false)} />
    </Paper>
  );
}
