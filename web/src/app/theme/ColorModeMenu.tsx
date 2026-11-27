import CheckIcon from '@mui/icons-material/Check';
import ContrastIcon from '@mui/icons-material/Contrast';
import DarkModeOutlinedIcon from '@mui/icons-material/DarkModeOutlined';
import LightModeOutlinedIcon from '@mui/icons-material/LightModeOutlined';
import IconButton from '@mui/material/IconButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import { useColorScheme } from '@mui/material/styles';
import Tooltip from '@mui/material/Tooltip';
import { useState } from 'react';

type Mode = 'light' | 'dark' | 'system';

const systemMode = {
  value: 'system' as Mode,
  label: 'Як у системі',
  icon: ContrastIcon,
};

const modes: { value: Mode; label: string; icon: typeof ContrastIcon }[] = [
  { value: 'light', label: 'Світла', icon: LightModeOutlinedIcon },
  { value: 'dark', label: 'Темна', icon: DarkModeOutlinedIcon },
  systemMode,
];

/** Lets the user pick the light, dark or system theme; system is the default. */
export function ColorModeMenu() {
  const { mode, setMode } = useColorScheme();
  const [anchor, setAnchor] = useState<HTMLElement | null>(null);
  const current =
    modes.find((m) => m.value === (mode ?? 'system')) ?? systemMode;
  const CurrentIcon = current.icon;

  return (
    <>
      <Tooltip title={`Тема: ${current.label.toLowerCase()}`}>
        <IconButton
          color="inherit"
          aria-label="Тема оформлення"
          aria-haspopup="menu"
          aria-expanded={anchor ? 'true' : undefined}
          onClick={(event) => setAnchor(event.currentTarget)}
        >
          <CurrentIcon />
        </IconButton>
      </Tooltip>
      <Menu
        anchorEl={anchor}
        open={anchor !== null}
        onClose={() => setAnchor(null)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
      >
        {modes.map(({ value, label, icon: Icon }) => (
          <MenuItem
            key={value}
            selected={value === current.value}
            onClick={() => {
              setMode(value);
              setAnchor(null);
            }}
          >
            <ListItemIcon>
              <Icon fontSize="small" />
            </ListItemIcon>
            <ListItemText>{label}</ListItemText>
            {value === current.value && (
              <CheckIcon fontSize="small" sx={{ ml: 2 }} />
            )}
          </MenuItem>
        ))}
      </Menu>
    </>
  );
}
