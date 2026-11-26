import MenuItem from '@mui/material/MenuItem';
import Box from '@mui/material/Box';
import TextField from '@mui/material/TextField';
import ToggleButton from '@mui/material/ToggleButton';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import type { ReactNode } from 'react';
import type { GiftCategoryItem, Recipient } from '../data/gifts-api';
import { recipientLabels, recipients } from '../display/labels';
import type { GiftFilterState } from '../url-state/use-gift-filters';

interface GiftFiltersProps {
  state: GiftFilterState;
  categories: GiftCategoryItem[];
  /** The add button of the tab, on the same row. */
  action?: ReactNode;
}

/** «Для кого» (Все / Маша / Виталий / Вместе) and «Категория»; both live in the link. */
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
              sx={{ textTransform: 'none', px: 1.5 }}
            >
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
      {action && <Box sx={{ ml: { sm: 'auto' } }}>{action}</Box>}
    </Box>
  );
}
