import Box from '@mui/material/Box';
import ButtonBase from '@mui/material/ButtonBase';
import Checkbox from '@mui/material/Checkbox';
import Chip from '@mui/material/Chip';
import ListItem from '@mui/material/ListItem';
import Typography from '@mui/material/Typography';
import { useId } from 'react';
import { formatWhen, personLabel } from '../data/labels';
import type { ShoppingItem } from '../data/shopping-api';

interface ItemRowProps {
  item: ShoppingItem;
  onToggle: (item: ShoppingItem) => void;
  onOpen: (item: ShoppingItem) => void;
  divider?: boolean;
}

/**
 * One item: a big checkbox, the name with its quantity, the note and who added it; urgent ones carry «Терміново» and a
 * tinted edge. A bought item (in the cart) is struck through. Tapping the text opens the edit dialog.
 */
export function ItemRow({ item, onToggle, onOpen, divider }: ItemRowProps) {
  const nameId = useId();
  const bought = item.status === 'Bought';
  const caption = bought
    ? `купив(ла) ${personLabel(item.boughtBy)}${item.boughtAtUtc ? ` · ${formatWhen(item.boughtAtUtc)}` : ''}`
    : `додав(ла) ${personLabel(item.addedBy)}`;

  return (
    <ListItem
      divider={divider}
      disablePadding
      sx={(theme) => ({
        pl: 0.5,
        pr: 1,
        borderLeft: 3,
        borderLeftColor:
          item.urgent && !bought ? 'secondary.main' : 'transparent',
        bgcolor:
          item.urgent && !bought
            ? `rgba(${theme.vars.palette.secondary.mainChannel} / 0.08)`
            : undefined,
      })}
    >
      <Checkbox
        checked={bought}
        onChange={() => onToggle(item)}
        slotProps={{ input: { 'aria-labelledby': nameId } }}
        sx={{ p: 1.5, '& .MuiSvgIcon-root': { fontSize: 28 } }}
      />
      <ButtonBase
        onClick={() => onOpen(item)}
        aria-label={`Змінити: ${item.name}`}
        sx={{
          flex: 1,
          minWidth: 0,
          justifyContent: 'flex-start',
          textAlign: 'left',
          py: 1,
          borderRadius: 2,
        }}
      >
        <Box sx={{ minWidth: 0, display: 'grid', gap: 0.25 }}>
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 1,
              flexWrap: 'wrap',
            }}
          >
            <Typography
              id={nameId}
              sx={{
                fontWeight: 600,
                textDecoration: bought ? 'line-through' : 'none',
                color: bought ? 'text.secondary' : 'text.primary',
                overflowWrap: 'anywhere',
              }}
            >
              {item.name}
            </Typography>
            {item.quantity && (
              <Chip
                size="small"
                label={item.quantity}
                sx={{ fontVariantNumeric: 'tabular-nums' }}
              />
            )}
            {item.urgent && !bought && (
              <Chip size="small" color="secondary" label="Терміново" />
            )}
          </Box>
          {item.note && (
            <Typography
              variant="body2"
              sx={{ color: 'text.secondary', overflowWrap: 'anywhere' }}
            >
              {item.note}
            </Typography>
          )}
          <Typography variant="caption" sx={{ color: 'text.secondary' }}>
            {caption}
          </Typography>
        </Box>
      </ButtonBase>
    </ListItem>
  );
}
