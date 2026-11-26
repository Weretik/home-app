import List from '@mui/material/List';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import ListSubheader from '@mui/material/ListSubheader';
import Box from '@mui/material/Box';
import Divider from '@mui/material/Divider';
import Tooltip from '@mui/material/Tooltip';
import { NavLink } from 'react-router-dom';
import { mainSections, serviceSections, type Section } from './sections';

export const SIDE_NAV_WIDE = 260;
export const SIDE_NAV_NARROW = 84;

/**
 * The side menu on a computer, under the header: grouped sections with a caption each.
 * Collapsed, it keeps only the icons with a tooltip; the header button switches it.
 */
export function SideNav({ collapsed }: { collapsed: boolean }) {
  const group = (title: string, sections: Section[]) => (
    <List
      aria-label={title}
      subheader={
        collapsed ? undefined : (
          <ListSubheader
            disableSticky
            sx={{
              bgcolor: 'transparent',
              px: 1.5,
              lineHeight: '36px',
              typography: 'subtitle2',
              fontWeight: 600,
              color: 'text.primary',
            }}
          >
            {title}
          </ListSubheader>
        )
      }
      sx={{ display: 'grid', gap: 0.5, px: 2, py: 1 }}
    >
      {sections.map(({ path, label, icon: Icon }) => (
        <Tooltip
          key={path}
          title={collapsed ? label : ''}
          placement="right"
          disableInteractive
        >
          <ListItemButton
            component={NavLink}
            to={path}
            end={path === '/'}
            aria-label={collapsed ? label : undefined}
            sx={(theme) => ({
              borderRadius: 3,
              minHeight: 46,
              px: collapsed ? 0 : 1.5,
              justifyContent: collapsed ? 'center' : 'flex-start',
              color: 'text.secondary',
              '&:hover': {
                color: 'primary.main',
                bgcolor: `rgba(${theme.vars.palette.primary.mainChannel} / 0.08)`,
              },
              '&.active': {
                color: 'primary.main',
                fontWeight: 600,
                bgcolor: `rgba(${theme.vars.palette.primary.mainChannel} / 0.12)`,
              },
            })}
          >
            <ListItemIcon
              sx={{
                minWidth: collapsed ? 0 : 38,
                color: 'inherit',
              }}
            >
              <Icon />
            </ListItemIcon>
            {!collapsed && (
              <ListItemText
                primary={label}
                slotProps={{ primary: { sx: { fontWeight: 'inherit' } } }}
                sx={{ my: 0 }}
              />
            )}
          </ListItemButton>
        </Tooltip>
      ))}
    </List>
  );

  return (
    <Box
      component="nav"
      aria-label="Меню"
      sx={(theme) => ({
        display: { xs: 'none', md: 'block' },
        width: collapsed ? SIDE_NAV_NARROW : SIDE_NAV_WIDE,
        flex: 'none',
        position: 'sticky',
        top: 'var(--top-bar-height)',
        height: 'calc(100dvh - var(--top-bar-height))',
        overflowX: 'hidden',
        overflowY: 'auto',
        bgcolor: 'background.paper',
        transition: theme.transitions.create('width', {
          duration: theme.transitions.duration.shorter,
        }),
      })}
    >
      {group('Разделы', mainSections)}
      <Divider sx={{ mx: 2 }} />
      {group('Сервис', serviceSections)}
    </Box>
  );
}
