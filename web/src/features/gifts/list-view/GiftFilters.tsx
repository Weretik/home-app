import MenuItem from '@mui/material/MenuItem';
import Box from '@mui/material/Box';
import TextField from '@mui/material/TextField';
import ToggleButton from '@mui/material/ToggleButton';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import type { ReactNode } from 'react';
import type {
  GiftCategoryItem,
  GiftPriority,
  Recipient,
} from '../data/gifts-api';
import {
  priorities,
  priorityLabels,
  recipientLabels,
  recipients,
} from '../display/labels';
import { RecipientAvatar } from '../display/RecipientAvatar';
import { toneColor, toneTint } from '../display/recipient-tones';
import type { GiftFilterState } from '../url-state/use-gift-filters';

interface GiftFiltersProps {
  state: GiftFilterState;
  categories: GiftCategoryItem[];
  /** The add button of the tab, on the same row. */
  action?: ReactNode;
}

/** «Для кого» (Все / Маша / Виталий / Вместе, each in the person's colour), «Категория» and «Приоритет»; all live in the link. */
export function GiftFilters({ state, categories, action }: GiftFiltersProps) {
  const { filters } = state;

  return (
    <Box
      sx={{ display: 'flex', flexWrap: 'wrap', alignItems: 'center', gap: 1.5 }}
    >
      <Box sx={{ maxWidth: '100%', overflowX: 'auto' }}>
        <ToggleButtonGroup
          exclusive
          size="small"
          aria-label="Для кого"
          value={filters.recipient ?? 'all'}
          onChange={(_, value: Recipient | 'all' | null) => {
            if (value) state.setRecipient(value === 'all' ? undefined : value);
          }}
        >
          <ToggleButton value="all" sx={{ textTransform: 'none', px: 1.5 }}>
            Все
          </ToggleButton>
          {recipients.map((recipient) => (
            <ToggleButton
              key={recipient}
              value={recipient}
              sx={(theme) => ({
                textTransform: 'none',
                px: 1.5,
                gap: 0.75,
                '&.Mui-selected, &.Mui-selected:hover': {
                  color: 'text.primary',
                  fontWeight: 600,
                  bgcolor: toneTint(theme, recipient, 0.12),
                  borderColor: toneColor(theme, recipient),
                },
              })}
            >
              <RecipientAvatar recipient={recipient} size={20} />
              {recipientLabels[recipient]}
            </ToggleButton>
          ))}
        </ToggleButtonGroup>
      </Box>
      <TextField
        select
        size="small"
        label="Категория"
        value={filters.categoryId ?? ''}
        onChange={(event) => state.setCategory(event.target.value || undefined)}
        sx={{ minWidth: { xs: 160, sm: 200 } }}
        slotProps={{
          select: { displayEmpty: true },
          inputLabel: { shrink: true },
        }}
      >
        <MenuItem value="">Все</MenuItem>
        {categories.map((category) => (
          <MenuItem key={category.id} value={category.id}>
            {category.name}
          </MenuItem>
        ))}
      </TextField>
      <TextField
        select
        size="small"
        label="Приоритет"
        value={filters.priority ?? ''}
        onChange={(event) =>
          state.setPriority((event.target.value as GiftPriority) || undefined)
        }
        sx={{ minWidth: { xs: 160, sm: 180 } }}
        slotProps={{
          select: { displayEmpty: true },
          inputLabel: { shrink: true },
        }}
      >
        <MenuItem value="">Все</MenuItem>
        {priorities.map((priority) => (
          <MenuItem key={priority} value={priority}>
            {priorityLabels[priority]}
          </MenuItem>
        ))}
      </TextField>
      {action && <Box sx={{ ml: { sm: 'auto' } }}>{action}</Box>}
    </Box>
  );
}
