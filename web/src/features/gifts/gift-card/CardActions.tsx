import BlockOutlinedIcon from '@mui/icons-material/BlockOutlined';
import CheckCircleOutlineOutlinedIcon from '@mui/icons-material/CheckCircleOutlineOutlined';
import OpenInNewOutlinedIcon from '@mui/icons-material/OpenInNewOutlined';
import UndoOutlinedIcon from '@mui/icons-material/UndoOutlined';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import type { GiftItem, GiftStatus } from '../data/gifts-api';
import { RowActions } from '../editing/RowActions';

export interface GiftHandlers {
  onStatus: (gift: GiftItem, status: GiftStatus) => void;
  onEdit: (gift: GiftItem) => void;
  onDelete: (gift: GiftItem) => void;
}

interface CardActionsProps extends GiftHandlers {
  gift: GiftItem;
  /** «Список» offers «Подарено» and «Не актуально»; «Подаренные» offers «Вернуть в список». */
  variant: 'active' | 'archive';
  /** The wish's status change is being sent. */
  isPending?: boolean;
}

/** The bottom row of a wish card: its shop link, the status button and the «⋮» menu. */
export function CardActions({
  gift,
  variant,
  isPending,
  onStatus,
  onEdit,
  onDelete,
}: CardActionsProps) {
  const isActive = variant === 'active';
  const dropped = {
    label: 'Не актуально',
    icon: <BlockOutlinedIcon fontSize="small" />,
    onClick: () => onStatus(gift, 'Dropped'),
    disabled: isPending,
  };

  return (
    <Box
      sx={{ display: 'flex', flexWrap: 'wrap', alignItems: 'center', gap: 1 }}
    >
      {gift.link && (
        <Button
          size="small"
          href={gift.link}
          target="_blank"
          rel="noopener noreferrer"
          startIcon={<OpenInNewOutlinedIcon />}
        >
          Ссылка
        </Button>
      )}
      <Box sx={{ flex: 1 }} />
      <Button
        size="small"
        variant={isActive ? 'contained' : 'outlined'}
        disabled={isPending}
        startIcon={
          isActive ? <CheckCircleOutlineOutlinedIcon /> : <UndoOutlinedIcon />
        }
        onClick={() => onStatus(gift, isActive ? 'Gifted' : 'Wanted')}
      >
        {isActive ? 'Подарено' : 'Вернуть в список'}
      </Button>
      <RowActions
        label={gift.name}
        onEdit={() => onEdit(gift)}
        onDelete={() => onDelete(gift)}
        extra={isActive ? [dropped] : []}
      />
    </Box>
  );
}
