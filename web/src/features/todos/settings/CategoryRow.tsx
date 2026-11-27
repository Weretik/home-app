import DeleteOutlineOutlinedIcon from '@mui/icons-material/DeleteOutlineOutlined';
import EditOutlinedIcon from '@mui/icons-material/EditOutlined';
import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Tooltip from '@mui/material/Tooltip';
import type { TodoCategoryItem } from '../data/categories-api';
import { formatActiveCount } from './category-form';

/** One category: its emoji tile, name and active task count, then change and delete. */
export function CategoryRow({
  item,
  divider,
  onEdit,
  onDelete,
}: {
  item: TodoCategoryItem;
  divider: boolean;
  onEdit: () => void;
  onDelete: () => void;
}) {
  return (
    <ListItem divider={divider} sx={{ gap: 1.5, py: 1 }}>
      <Box
        aria-hidden
        sx={(theme) => ({
          width: 40,
          height: 40,
          flexShrink: 0,
          borderRadius: 3,
          display: 'grid',
          placeItems: 'center',
          fontSize: 20,
          bgcolor: `rgba(${theme.vars.palette.primary.mainChannel} / 0.12)`,
        })}
      >
        {item.emoji ?? '•'}
      </Box>
      <ListItemText
        sx={{ my: 0, minWidth: 0, overflowWrap: 'anywhere' }}
        primary={item.name}
        secondary={formatActiveCount(item.activeCount)}
      />
      <Tooltip title="Змінити">
        <IconButton aria-label={`Змінити: ${item.name}`} onClick={onEdit}>
          <EditOutlinedIcon fontSize="small" />
        </IconButton>
      </Tooltip>
      <Tooltip title="Видалити">
        <IconButton aria-label={`Видалити: ${item.name}`} onClick={onDelete}>
          <DeleteOutlineOutlinedIcon fontSize="small" />
        </IconButton>
      </Tooltip>
    </ListItem>
  );
}
