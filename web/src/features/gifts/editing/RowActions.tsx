import DeleteOutlineOutlinedIcon from '@mui/icons-material/DeleteOutlineOutlined';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
import MoreVertOutlinedIcon from '@mui/icons-material/MoreVertOutlined';
import IconButton from '@mui/material/IconButton';
import ListItemIcon from '@mui/material/ListItemIcon';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import Tooltip from '@mui/material/Tooltip';
import { useState, type ReactNode } from 'react';

export interface RowAction {
  label: string;
  icon: ReactNode;
  onClick: () => void;
  /** A destructive action, shown in red. */
  danger?: boolean;
  /** Unavailable for now, e.g. while the same change is being sent. */
  disabled?: boolean;
}

interface RowActionsProps {
  /** What the row is, for the button's label: «Серьги с топазом». */
  label: string;
  onEdit: () => void;
  onDelete: () => void;
  /** Actions shown before «Изменить» and «Удалить» (e.g. «Подарено»). */
  extra?: RowAction[];
}

/** The «⋮» button of a row: any extra actions, then «Изменить» and «Удалить». */
export function RowActions({
  label,
  onEdit,
  onDelete,
  extra = [],
}: RowActionsProps) {
  const [anchor, setAnchor] = useState<HTMLElement | null>(null);
  const actions: RowAction[] = [
    ...extra,
    {
      label: 'Изменить',
      icon: <EditOutlinedIcon fontSize="small" />,
      onClick: onEdit,
    },
    {
      label: 'Удалить',
      icon: <DeleteOutlineOutlinedIcon fontSize="small" />,
      onClick: onDelete,
      danger: true,
    },
  ];

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
        {actions.map((action) => (
          <MenuItem
            key={action.label}
            disabled={action.disabled}
            onClick={() => {
              setAnchor(null);
              action.onClick();
            }}
            sx={action.danger ? { color: 'error.main' } : undefined}
          >
            <ListItemIcon sx={action.danger ? { color: 'inherit' } : undefined}>
              {action.icon}
            </ListItemIcon>
            {action.label}
          </MenuItem>
        ))}
      </Menu>
    </>
  );
}
