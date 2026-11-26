import ArrowDownwardOutlinedIcon from '@mui/icons-material/ArrowDownwardOutlined';
import ArrowUpwardOutlinedIcon from '@mui/icons-material/ArrowUpwardOutlined';
import IconButton from '@mui/material/IconButton';
import ListItem from '@mui/material/ListItem';
import ListItemText from '@mui/material/ListItemText';
import Tooltip from '@mui/material/Tooltip';
import { RowMenu } from '../page-parts/RowMenu';
import type { Department } from './departments-api';

interface DepartmentRowProps {
  department: Department;
  isFirst: boolean;
  isLast: boolean;
  /** A reorder is being saved: the arrows wait. */
  isMoving: boolean;
  onMove: (step: -1 | 1) => void;
  onEdit: () => void;
  onDelete: () => void;
}

const words = (n: number) => {
  const tens = n % 100;
  const ones = n % 10;
  if (tens >= 11 && tens <= 14) return `${n} слов`;
  if (ones === 1) return `${n} слово`;
  if (ones >= 2 && ones <= 4) return `${n} слова`;
  return `${n} слов`;
};

/** A department in store order: icon and name, its counts, up / down arrows and «⋮» (the fallback cannot be deleted). */
export function DepartmentRow(props: DepartmentRowProps) {
  const { department: d, isFirst, isLast, isMoving, onMove } = props;
  const caption = [
    words(d.keywordCount),
    d.neededCount > 0 ? `${d.neededCount} в списке` : null,
    d.isFallback ? 'сюда идёт всё без отдела' : null,
  ]
    .filter(Boolean)
    .join(' · ');

  return (
    <ListItem divider sx={{ pr: 1, gap: 0.5 }}>
      <ListItemText
        primary={`${d.icon ? `${d.icon} ` : ''}${d.name}`}
        secondary={caption}
        slotProps={{ primary: { sx: { fontWeight: 600 } } }}
      />
      <Tooltip title="Выше">
        <span>
          <IconButton
            aria-label={`Выше: ${d.name}`}
            disabled={isFirst || isMoving}
            onClick={() => onMove(-1)}
          >
            <ArrowUpwardOutlinedIcon fontSize="small" />
          </IconButton>
        </span>
      </Tooltip>
      <Tooltip title="Ниже">
        <span>
          <IconButton
            aria-label={`Ниже: ${d.name}`}
            disabled={isLast || isMoving}
            onClick={() => onMove(1)}
          >
            <ArrowDownwardOutlinedIcon fontSize="small" />
          </IconButton>
        </span>
      </Tooltip>
      <RowMenu
        name={d.name}
        onEdit={props.onEdit}
        onDelete={d.isFallback ? undefined : props.onDelete}
      />
    </ListItem>
  );
}
