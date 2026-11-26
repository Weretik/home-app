import DeleteOutlineOutlinedIcon from '@mui/icons-material/DeleteOutlineOutlined';
import ReplayOutlinedIcon from '@mui/icons-material/ReplayOutlined';
import Card from '@mui/material/Card';
import IconButton from '@mui/material/IconButton';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Tooltip from '@mui/material/Tooltip';
import Typography from '@mui/material/Typography';
import { useId } from 'react';
import { itemTitle, personLabel } from '../data/labels';
import type { ShoppingItem } from '../data/shopping-api';
import type { HistoryDay as Day } from './history-groups';

interface HistoryDayProps {
  day: Day;
  /** The item being returned to the list: its button waits. */
  returning: string | null;
  onReturn: (item: ShoppingItem) => void;
  onDelete: (item: ShoppingItem) => void;
}

const time = new Intl.DateTimeFormat('ru-RU', {
  hour: '2-digit',
  minute: '2-digit',
});

/** One day of the history: what was bought, by whom and when, with «Вернуть в список» and delete (R-10). */
export function HistoryDay({
  day,
  returning,
  onReturn,
  onDelete,
}: HistoryDayProps) {
  const titleId = useId();
  return (
    <Card component="section" aria-labelledby={titleId}>
      <Typography
        id={titleId}
        variant="subtitle1"
        component="h3"
        sx={{
          fontWeight: 600,
          px: 2,
          pt: 1.5,
          '&::first-letter': { textTransform: 'uppercase' },
        }}
      >
        {day.label}
      </Typography>
      <List aria-labelledby={titleId}>
        {day.items.map((item, index) => (
          <ListItem
            key={item.id}
            divider={index < day.items.length - 1}
            sx={{ gap: 0.5, pr: 1 }}
          >
            <ListItemText
              primary={itemTitle(item)}
              secondary={[
                item.departmentIcon
                  ? `${item.departmentIcon} ${item.departmentName}`
                  : item.departmentName,
                `${personLabel(item.boughtBy)}${item.boughtAtUtc ? ` · ${time.format(new Date(item.boughtAtUtc))}` : ''}`,
              ].join(' · ')}
              slotProps={{
                primary: { sx: { fontWeight: 600, overflowWrap: 'anywhere' } },
              }}
            />
            <Tooltip title="Вернуть в список">
              <span>
                <IconButton
                  aria-label={`Вернуть в список: ${item.name}`}
                  disabled={returning === item.id}
                  onClick={() => onReturn(item)}
                >
                  <ReplayOutlinedIcon fontSize="small" />
                </IconButton>
              </span>
            </Tooltip>
            <Tooltip title="Удалить">
              <IconButton
                aria-label={`Удалить: ${item.name}`}
                onClick={() => onDelete(item)}
              >
                <DeleteOutlineOutlinedIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          </ListItem>
        ))}
      </List>
    </Card>
  );
}
