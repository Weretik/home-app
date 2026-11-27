import BlockOutlinedIcon from '@mui/icons-material/BlockOutlined';
import CheckCircleOutlineOutlinedIcon from '@mui/icons-material/CheckCircleOutlineOutlined';
import UndoOutlinedIcon from '@mui/icons-material/UndoOutlined';
import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import type { GiftItem, GiftStatus } from '../data/gifts-api';
import { RowActions, type RowAction } from '../editing/RowActions';

export interface GiftHandlers {
  onStatus: (gift: GiftItem, status: GiftStatus) => void;
  onEdit: (gift: GiftItem) => void;
  onDelete: (gift: GiftItem) => void;
}

interface GiftActionsProps extends GiftHandlers {
  gift: GiftItem;
  /** «Список» offers «Подаровано» and «Неактуально»; «Подаровані» offers «Повернути до списку». */
  variant: 'active' | 'archive';
  /** The gift's status change is being sent. */
  isPending: boolean;
  /** A phone row: every action in the «⋮» menu instead of a quick button beside it. */
  compact?: boolean;
}

/** The actions of a gift row: the quick status button (computer) and «⋮» with the rest. */
export function GiftActions({
  gift,
  variant,
  isPending,
  compact,
  onStatus,
  onEdit,
  onDelete,
}: GiftActionsProps) {
  const isActive = variant === 'active';
  const main: RowAction = isActive
    ? {
        label: 'Подаровано',
        icon: <CheckCircleOutlineOutlinedIcon fontSize="small" />,
        onClick: () => onStatus(gift, 'Gifted'),
        disabled: isPending,
      }
    : {
        label: 'Повернути до списку',
        icon: <UndoOutlinedIcon fontSize="small" />,
        onClick: () => onStatus(gift, 'Wanted'),
        disabled: isPending,
      };
  const dropped: RowAction = {
    label: 'Неактуально',
    icon: <BlockOutlinedIcon fontSize="small" />,
    onClick: () => onStatus(gift, 'Dropped'),
    disabled: isPending,
  };
  const extra = [...(compact ? [main] : []), ...(isActive ? [dropped] : [])];

  return (
    <Box
      sx={{ display: 'flex', alignItems: 'center', justifyContent: 'flex-end' }}
    >
      {!compact && (
        <Tooltip title={main.label}>
          <span>
            <IconButton
              size="small"
              color="primary"
              aria-label={`${main.label}: ${gift.name}`}
              disabled={isPending}
              onClick={main.onClick}
            >
              {main.icon}
            </IconButton>
          </span>
        </Tooltip>
      )}
      <RowActions
        label={gift.name}
        onEdit={() => onEdit(gift)}
        onDelete={() => onDelete(gift)}
        extra={extra}
      />
    </Box>
  );
}
