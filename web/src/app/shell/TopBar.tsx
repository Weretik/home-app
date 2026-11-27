import MenuOpenIcon from '@mui/icons-material/MenuOpen';
import MenuIcon from '@mui/icons-material/Menu';
import Box from '@mui/material/Box';
import ButtonBase from '@mui/material/ButtonBase';
import Toolbar from '@mui/material/Toolbar';
import Tooltip from '@mui/material/Tooltip';
import { ColorModeMenu } from '../theme/ColorModeMenu';
import { Brand } from './Brand';
import { SIDE_NAV_WIDE } from './SideNav';

interface TopBarProps {
  menuCollapsed: boolean;
  onToggleMenu: () => void;
}

/**
 * The full-width header: the logo, the button that collapses the side menu (computer only),
 * and the theme switch on the right.
 */
export function TopBar({ menuCollapsed, onToggleMenu }: TopBarProps) {
  const label = menuCollapsed ? 'Розгорнути меню' : 'Згорнути меню';

  return (
    <Box
      component="header"
      sx={(theme) => ({
        position: 'sticky',
        top: 0,
        zIndex: theme.zIndex.appBar,
        bgcolor: 'background.paper',
        pt: 'env(safe-area-inset-top, 0px)',
      })}
    >
      <Toolbar
        disableGutters
        sx={{
          height: 'var(--top-bar-height)',
          minHeight: 'auto !important',
          px: 2,
          gap: 1.5,
        }}
      >
        <Box sx={{ width: { md: SIDE_NAV_WIDE - 32 }, flex: 'none' }}>
          <Brand />
        </Box>
        <Tooltip title={label}>
          <ButtonBase
            onClick={onToggleMenu}
            aria-label={label}
            aria-pressed={!menuCollapsed}
            sx={(theme) => ({
              display: { xs: 'none', md: 'grid' },
              placeItems: 'center',
              width: 36,
              height: 36,
              borderRadius: 2,
              color: 'primary.main',
              bgcolor: `rgba(${theme.vars.palette.primary.mainChannel} / 0.1)`,
              transition: theme.transitions.create([
                'background-color',
                'color',
              ]),
              '&:hover': {
                bgcolor: 'primary.main',
                color: 'primary.contrastText',
              },
            })}
          >
            {menuCollapsed ? (
              <MenuIcon fontSize="small" />
            ) : (
              <MenuOpenIcon fontSize="small" />
            )}
          </ButtonBase>
        </Tooltip>
        <Box sx={{ flex: 1 }} />
        <ColorModeMenu />
      </Toolbar>
    </Box>
  );
}
