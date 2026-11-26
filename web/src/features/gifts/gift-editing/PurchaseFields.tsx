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

/** «Кто дарит» and «Купить до» (gifts 002 FR-001): with both set the server makes a to-do and a Telegram reminder. */
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
          label="Кто дарит"
          value={values.giver}
          onChange={(event) => set('giver', event.target.value as Giver | '')}
        >
          <MenuItem value="">
            <em>не указано</em>
          </MenuItem>
          {recipients.map((giver) => (
            <MenuItem key={giver} value={giver}>
              {recipientLabels[giver]}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          label="Купить до"
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
          ? 'Задача появится в «Делах», напоминание придёт в Телеграм за 2 недели.'
          : 'Укажите, кто дарит, и дату — появится задача в «Делах» и напоминание в Телеграме за 2 недели.'}
      </Typography>
    </Box>
  );
}
