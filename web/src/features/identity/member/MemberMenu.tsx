import LogoutIcon from '@mui/icons-material/Logout';
import Box from '@mui/material/Box';
import Divider from '@mui/material/Divider';
import IconButton from '@mui/material/IconButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import ListItemText from '@mui/material/ListItemText';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import { useState } from 'react';
import { useCurrentMember, useSignOut } from '../data/identity-api';
import { MemberAvatar } from './MemberAvatar';

/** The member's avatar in the header; its menu shows the name, the e-mail and «Вийти» (FR-006, FR-007). */
export function MemberMenu() {
  const { data: member } = useCurrentMember();
  const signOut = useSignOut();
  const [anchor, setAnchor] = useState<HTMLElement | null>(null);
  const label = member ? `Акаунт: ${member.name}` : 'Акаунт';

  return (
    <>
      <Tooltip title={label}>
        <IconButton
          aria-label={label}
          aria-haspopup="menu"
          aria-expanded={anchor ? 'true' : undefined}
          onClick={(event) => setAnchor(event.currentTarget)}
          sx={{ p: 0.5 }}
        >
          <MemberAvatar member={member} />
        </IconButton>
      </Tooltip>
      <Menu
        anchorEl={anchor}
        open={anchor !== null}
        onClose={() => setAnchor(null)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
      >
        {member && (
          <Box sx={{ px: 2, py: 1, maxWidth: 280 }}>
            <Typography variant="subtitle2" noWrap>
              {member.name}
            </Typography>
            <Typography variant="body2" noWrap sx={{ color: 'text.secondary' }}>
              {member.email}
            </Typography>
          </Box>
        )}
        {member && <Divider />}
        <MenuItem
          disabled={signOut.isPending}
          onClick={() => {
            setAnchor(null);
            signOut.mutate();
          }}
        >
          <ListItemIcon>
            <LogoutIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Вийти</ListItemText>
        </MenuItem>
      </Menu>
    </>
  );
}
