import Box from '@mui/material/Box';
import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import { BottomNav } from '../navigation/BottomNav';
import { SideNav } from '../navigation/SideNav';
import { TopBar } from './TopBar';

const COLLAPSED_KEY = 'home.side-nav.collapsed';

function readCollapsed() {
  try {
    return localStorage.getItem(COLLAPSED_KEY) === 'true';
  } catch {
    return false;
  }
}

function saveCollapsed(value: boolean) {
  try {
    localStorage.setItem(COLLAPSED_KEY, String(value));
  } catch {
    // Private mode: the menu still works, the choice is just not remembered.
  }
}

/**
 * The app frame (docs/design.md, "Layout"). Header on top across the whole width; below it, on a computer,
 * the side menu and the page on a tinted rounded panel. On a phone: header, page, bottom bar.
 * A new section is added in sections.ts and in the routes.
 */
export function AppLayout() {
  const [collapsed, setCollapsed] = useState(readCollapsed);

  const toggle = () => {
    saveCollapsed(!collapsed);
    setCollapsed(!collapsed);
  };

  return (
    <Box
      sx={{
        '--top-bar-height': { xs: '64px', md: '80px' },
        minHeight: '100dvh',
        bgcolor: 'background.paper',
      }}
    >
      <TopBar menuCollapsed={collapsed} onToggleMenu={toggle} />
      <Box sx={{ display: 'flex', alignItems: 'flex-start' }}>
        <SideNav collapsed={collapsed} />
        <Box
          component="main"
          sx={{
            flex: 1,
            minWidth: 0,
            minHeight: 'calc(100dvh - var(--top-bar-height))',
            bgcolor: 'background.default',
            borderRadius: { xs: '20px 20px 0 0', md: '16px 16px 0 0' },
            mr: { md: 2.5 },
            px: { xs: 2, md: 3 },
            pt: { xs: 2.5, md: 3 },
            pb: {
              xs: 'calc(96px + env(safe-area-inset-bottom, 0px))',
              md: 5,
            },
          }}
        >
          <Box sx={{ maxWidth: 1600 }}>
            <Outlet />
          </Box>
        </Box>
      </Box>
      <BottomNav />
    </Box>
  );
}
