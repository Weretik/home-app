import MoreVertOutlinedIcon from '@mui/icons-material/MoreVertOutlined';
import IconButton from '@mui/material/IconButton';
import Menu from '@mui/material/Menu';
import MenuItem from '@mui/material/MenuItem';
import Tooltip from '@mui/material/Tooltip';
import { useState } from 'react';

interface RowMenuProps {
  /** Names the row for the button's label: «Дії: Молочка». */
  name: string;
  onEdit: () => void;
  /** Left out when the row cannot be deleted (the fallback department). */
  onDelete?: () => void;
}

/** The «⋮» button at the end of a row with «Змінити» and «Видалити». */
export function RowMenu({ name, onEdit, onDelete }: RowMenuProps) {
  const [anchor, setAnchor] = useState<HTMLElement | null>(null);
  const pick = (action: () => void) => () => {
    setAnchor(null);
    action();
  };

  return (
    <>
      <Tooltip title="Дії">
        <IconButton
          aria-label={`Дії: ${name}`}
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
        <MenuItem onClick={pick(onEdit)}>Змінити</MenuItem>
        {onDelete && <MenuItem onClick={pick(onDelete)}>Видалити</MenuItem>}
      </Menu>
    </>
  );
}
