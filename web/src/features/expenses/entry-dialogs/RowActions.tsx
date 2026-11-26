import DeleteOutlineOutlinedIcon from '@mui/icons-material/DeleteOutlineOutlined';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
import MoreVertOutlinedIcon from '@mui/icons-material/MoreVertOutlined';
import IconButton from '@mui/material/IconButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import Tooltip from '@mui/material/Tooltip';
import { useState, type ReactNode } from 'react';

/** One more step a row offers before «Изменить» and «Удалить»: «Записать как расход». */
export interface RowAction {
  label: string;
  icon: ReactNode;
  onClick: () => void;
}

interface RowActionsProps {
  /** What the row is, for the button's label: «Продукты, 1 234,50 ₴». */
  label: string;
  onEdit: () => void;
  onDelete: () => void;
  /** Shown first in the menu. */
  extra?: RowAction;
}

/** The «⋮» button of a list row with «Изменить» and «Удалить», and an optional step of its own first. */
export function RowActions({
  label,
  onEdit,
  onDelete,
  extra,
}: RowActionsProps) {
  const [anchor, setAnchor] = useState<HTMLElement | null>(null);
  const pick = (action: () => void) => () => {
    setAnchor(null);
    action();
  };

  return (
    <>
      <Tooltip title="Действия">
        <IconButton
          size="small"
          aria-label={`Действия: ${label}`}
          aria-haspopup="menu"
          aria-expanded={anchor ? 'true' : undefined}
          onClick={(event) => setAnchor(event.currentTarget)}
        >
          <MoreVertOutlinedIcon fontSize="small" />
        </IconButton>
      </Tooltip>
      <Menu
        anchorEl={anchor}
        open={Boolean(anchor)}
        onClose={() => setAnchor(null)}
      >
        {extra && (
          <MenuItem onClick={pick(extra.onClick)}>
            <ListItemIcon>{extra.icon}</ListItemIcon>
            {extra.label}
          </MenuItem>
        )}
        <MenuItem onClick={pick(onEdit)}>
          <ListItemIcon>
            <EditOutlinedIcon fontSize="small" />
          </ListItemIcon>
          Изменить
        </MenuItem>
        <MenuItem onClick={pick(onDelete)} sx={{ color: 'error.main' }}>
          <ListItemIcon sx={{ color: 'inherit' }}>
            <DeleteOutlineOutlinedIcon fontSize="small" />
          </ListItemIcon>
          Удалить
        </MenuItem>
      </Menu>
    </>
  );
}
