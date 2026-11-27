import Box from '@mui/material/Box';
import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import type { Giver } from '../data/gifts-api';
import { recipientLabels, recipients } from '../display/labels';
import type { FieldErrors } from '../editing/field-checks';
import type { GiftFormValues } from './gift-form';

interface PurchaseFieldsProps {
  values: GiftFormValues;
  errors: FieldErrors;
  set: <K extends keyof GiftFormValues>(
    name: K,
    value: GiftFormValues[K],
  ) => void;
}

/** «Хто дарує» and «Купити до» (gifts 002 FR-001): with both set the server makes a to-do and a Telegram reminder. */
export function PurchaseFields({ values, errors, set }: PurchaseFieldsProps) {
  const both = Boolean(values.giver && values.buyBy);
  return (
    <Box sx={{ display: 'grid', gap: 1 }}>
      <Box
        sx={{
          display: 'grid',
          gap: 2,
          gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' },
        }}
      >
        <TextField
          select
          label="Хто дарує"
          value={values.giver}
          onChange={(event) => set('giver', event.target.value as Giver | '')}
        >
          <MenuItem value="">
            <em>не вказано</em>
          </MenuItem>
          {recipients.map((giver) => (
            <MenuItem key={giver} value={giver}>
              {recipientLabels[giver]}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          label="Купити до"
          type="date"
          value={values.buyBy}
          onChange={(event) => set('buyBy', event.target.value)}
          error={Boolean(errors.buyBy)}
          helperText={errors.buyBy}
          slotProps={{ inputLabel: { shrink: true } }}
        />
      </Box>
      <Typography variant="body2" color="text.secondary">
        {both
          ? 'Завдання з’явиться у «Справах», нагадування прийде в Телеграм за 2 тижні.'
          : 'Вкажіть, хто дарує, і дату — з’явиться завдання у «Справах» і нагадування в Телеграмі за 2 тижні.'}
      </Typography>
    </Box>
  );
}
