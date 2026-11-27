import Drawer from '@mui/material/Drawer';
import List from '@mui/material/List';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import ListSubheader from '@mui/material/ListSubheader';
import { NavLink } from 'react-router-dom';
import { phoneMoreSections } from './sections';

interface MoreSheetProps {
  open: boolean;
  onClose: () => void;
}

/**
 * The phone's «Ще» sheet: every section that is not in the bottom bar («Про нас», «Телеграм», «Стан», «Профіль»),
 * sliding up from the bottom (web ADR 0019). A tap opens the page and closes the sheet.
 */
export function MoreSheet({ open, onClose }: MoreSheetProps) {
  return (
    <Drawer
      anchor="bottom"
      open={open}
      onClose={onClose}
      slotProps={{
        paper: {
          sx: {
            borderRadius: '20px 20px 0 0',
            pb: 'calc(8px + env(safe-area-inset-bottom, 0px))',
          },
        },
      }}
    >
      <List
        aria-label="Ще розділи"
        subheader={
          <ListSubheader
            sx={{ bgcolor: 'transparent', typography: 'subtitle2', pt: 1 }}
          >
            Ще
          </ListSubheader>
        }
        sx={{ px: 1 }}
      >
        {phoneMoreSections.map(({ path, label, icon: Icon }) => (
          <ListItemButton
            key={path}
            component={NavLink}
            to={path}
            onClick={onClose}
            sx={(theme) => ({
              borderRadius: 3,
              minHeight: 52,
              '&.active': {
                color: 'primary.main',
                fontWeight: 600,
                bgcolor: `rgba(${theme.vars.palette.primary.mainChannel} / 0.12)`,
              },
            })}
          >
            <ListItemIcon sx={{ minWidth: 44, color: 'inherit' }}>
              <Icon />
            </ListItemIcon>
            <ListItemText
              primary={label}
              slotProps={{ primary: { sx: { fontWeight: 'inherit' } } }}
            />
          </ListItemButton>
        ))}
      </List>
    </Drawer>
  );
}
