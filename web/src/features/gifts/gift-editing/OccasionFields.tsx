import Box from '@mui/material/Box';
import TextField from '@mui/material/TextField';
import type { FieldErrors } from '../editing/field-checks';
import type { GiftFormValues } from './gift-form';

interface OccasionFieldsProps {
  values: GiftFormValues;
  errors: FieldErrors;
  set: <K extends keyof GiftFormValues>(
    name: K,
    value: GiftFormValues[K],
  ) => void;
}

/** When the gift is for («повод к дате») and free notes. */
export function OccasionFields({ values, errors, set }: OccasionFieldsProps) {
  return (
    <>
      <Box
        sx={{
          display: 'grid',
          gap: 2,
          gridTemplateColumns: { xs: '1fr', sm: '3fr 2fr' },
        }}
      >
        <TextField
          label="Повод"
          placeholder="День рождения"
          value={values.occasion}
          onChange={(event) => set('occasion', event.target.value)}
          error={Boolean(errors.occasion)}
          helperText={errors.occasion}
        />
        <TextField
          label="Дата повода"
          type="date"
          value={values.occasionDate}
          onChange={(event) => set('occasionDate', event.target.value)}
          error={Boolean(errors.occasionDate)}
          helperText={errors.occasionDate}
          slotProps={{ inputLabel: { shrink: true } }}
        />
      </Box>
      <TextField
        label="Заметки"
        value={values.notes}
        multiline
        minRows={2}
        onChange={(event) => set('notes', event.target.value)}
        error={Boolean(errors.notes)}
        helperText={errors.notes}
      />
    </>
  );
}
