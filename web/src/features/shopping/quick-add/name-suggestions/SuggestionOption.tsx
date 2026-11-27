import Box from '@mui/material/Box';
import Chip from '@mui/material/Chip';
import Typography from '@mui/material/Typography';
import type { HTMLAttributes } from 'react';
import type { NameSuggestion } from '../../data/shopping-api';
import { suggestionCaption } from './suggestion-caption';

interface SuggestionOptionProps {
  /** The props Autocomplete gives the option's `<li>` (role, id, handlers), without its key. */
  props: HTMLAttributes<HTMLLIElement>;
  option: NameSuggestion;
}

/** One suggestion (FR-203): the department icon and the name, «Відділ · брали N разів», and «вже в списку». */
export function SuggestionOption({ props, option }: SuggestionOptionProps) {
  return (
    <Box
      component="li"
      {...props}
      sx={{ display: 'flex', alignItems: 'center', gap: 1.5, py: 1 }}
    >
      <Box component="span" aria-hidden sx={{ fontSize: 20, flex: 'none' }}>
        {option.departmentIcon || '🛒'}
      </Box>
      <Box sx={{ minWidth: 0, flex: 1 }}>
        <Typography sx={{ fontWeight: 600, overflowWrap: 'anywhere' }}>
          {option.name}
        </Typography>
        <Typography variant="body2" sx={{ color: 'text.secondary' }}>
          {suggestionCaption(option)}
        </Typography>
      </Box>
      {option.inList && (
        <Chip
          label="вже в списку"
          size="small"
          variant="outlined"
          sx={{ flex: 'none' }}
        />
      )}
    </Box>
  );
}
