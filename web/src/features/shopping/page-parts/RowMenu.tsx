import MoreVertOutlinedIcon from '@mui/icons-material/MoreVertOutlined';
import IconButton from '@mui/material/IconButton';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import Tooltip from '@mui/material/Tooltip';
import { useState } from 'react';

interface RowMenuProps {
  /** Names the row for the button's label: «Действия: Молочка». */
  name: string;
  onEdit: () => void;
  /** Left out when the row cannot be deleted (the fallback department). */
  onDelete?: () => void;
}

/** The «⋮» button at the end of a row with «Изменить» and «Удалить». */
export function RowMenu({ name, onEdit, onDelete }: RowMenuProps) {
  const [anchor, setAnchor] = useState<HTMLElement | null>(null);
  const pick = (action: () => void) => () => {
    setAnchor(null);
    action();
  };

  return (
    <>
      <Tooltip title="Действия">
        <IconButton
          aria-label={`Действия: ${name}`}
          onClick={(event) => setAnchor(event.currentTarget)}
        >
          <MoreVertOutlinedIcon fontSize="small" />
        </IconButton>
      </Tooltip>
      <Menu
        anchorEl={anchor}
        open={anchor !== null}
        onClose={() => setAnchor(null)}
      >
        <MenuItem onClick={pick(onEdit)}>Изменить</MenuItem>
        {onDelete && <MenuItem onClick={pick(onDelete)}>Удалить</MenuItem>}
      </Menu>
    </>
  );
}
