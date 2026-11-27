import DeleteOutlineOutlinedIcon from '@mui/icons-material/DeleteOutlineOutlined';
import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import TextField from '@mui/material/TextField';
import Tooltip from '@mui/material/Tooltip';
import { useEffect, useRef } from 'react';
import type { FieldErrors } from '../editing/field-checks';
import type { VariantValues } from '../gift-editing/gift-form';
import { PriceField } from './PriceField';

interface VariantRowProps {
  variant: VariantValues;
  index: number;
  errors: FieldErrors;
  /** Move focus to the link of a row that was just added. */
  focusLink: boolean;
  onChange: (patch: Partial<VariantValues>) => void;
  onRemove: () => void;
}

/** One variant: its link, then a title and a price, and a button that removes it. */
export function VariantRow({
  variant,
  index,
  errors,
  focusLink,
  onChange,
  onRemove,
}: VariantRowProps) {
  const linkRef = useRef<HTMLInputElement>(null);
  // Focus once, when the row appears after «Додати варіант».
  useEffect(() => {
    if (focusLink) linkRef.current?.focus();
  }, [focusLink]);
  const at = `variants[${index}]`;
  const number = index + 1;

  return (
    <Box
      component="li"
      sx={{
        display: 'grid',
        gap: 1,
        pb: 1.5,
        borderBottom: 1,
        borderColor: 'divider',
      }}
    >
      <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1 }}>
        <TextField
          label={`Посилання варіанта ${number}`}
          value={variant.link}
          size="small"
          fullWidth
          inputRef={linkRef}
          onChange={(event) => onChange({ link: event.target.value })}
          error={Boolean(errors[`${at}.link`])}
          helperText={errors[`${at}.link`]}
          slotProps={{ htmlInput: { inputMode: 'url' } }}
        />
        <Tooltip title="Видалити варіант">
          <IconButton
            aria-label={`Видалити варіант ${number}`}
            onClick={onRemove}
          >
            <DeleteOutlineOutlinedIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      </Box>
      <Box
        sx={{
          display: 'grid',
          gap: 1,
          gridTemplateColumns: { xs: '1fr', sm: '2fr 1fr' },
          pr: { sm: 6 },
        }}
      >
        <TextField
          label="Назва варіанта"
          value={variant.title}
          size="small"
          onChange={(event) => onChange({ title: event.target.value })}
          error={Boolean(errors[`${at}.title`])}
          helperText={errors[`${at}.title`]}
        />
        <PriceField
          label="Ціна варіанта"
          size="small"
          value={variant.price}
          error={errors[`${at}.price`]}
          onChange={(price) => onChange({ price })}
        />
      </Box>
    </Box>
  );
}
