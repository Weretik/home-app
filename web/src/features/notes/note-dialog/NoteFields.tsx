import FormControlLabel from '@mui/material/FormControlLabel';
import MenuItem from '@mui/material/MenuItem';
import Switch from '@mui/material/Switch';
import TextField from '@mui/material/TextField';
import { categoryLabel, useCategories } from '../data/categories-api';
import { kindLabels, kinds, people, personNames } from '../data/labels';
import { SegmentedField } from './SegmentedField';
import type { FieldErrors, NoteFormValues } from './note-form';

const persons = people.map((value) => ({ value, label: personNames[value] }));
const kindOptions = kinds.map((value) => ({ value, label: kindLabels[value] }));

const message = (errors: FieldErrors, name: string) => ({
  error: Boolean(errors[name]),
  helperText: errors[name],
});

interface NoteFieldsProps {
  values: NoteFormValues;
  errors: FieldErrors;
  set: <K extends keyof NoteFormValues>(
    name: K,
    value: NoteFormValues[K],
  ) => void;
}

/** The fields of the note form, one per row (FR-005). */
export function NoteFields({ values, errors, set }: NoteFieldsProps) {
  const categories = useCategories();
  const known =
    values.categoryId === '' ||
    (categories.data ?? []).some((c) => c.id === values.categoryId);

  return (
    <>
      <SegmentedField
        label="Про кого"
        value={values.about}
        options={persons}
        onChange={(value) => set('about', value)}
      />
      <SegmentedField
        label="Що"
        value={values.kind}
        options={kindOptions}
        onChange={(value) => set('kind', value)}
      />
      <TextField
        label="Що саме"
        required
        multiline
        value={values.title}
        onChange={(event) => set('title', event.target.value)}
        {...message(errors, 'title')}
      />
      <TextField
        select
        label="Категорія"
        value={known ? values.categoryId : ''}
        disabled={categories.isPending}
        onChange={(event) => set('categoryId', event.target.value)}
        {...message(errors, 'categoryId')}
        slotProps={{
          select: { displayEmpty: true },
          inputLabel: { shrink: true },
        }}
      >
        <MenuItem value="">Без категорії</MenuItem>
        {(categories.data ?? []).map((category) => (
          <MenuItem key={category.id} value={category.id}>
            {categoryLabel(category)}
          </MenuItem>
        ))}
      </TextField>
      <TextField
        label="Де купити / бренд"
        value={values.place}
        onChange={(event) => set('place', event.target.value)}
        {...message(errors, 'place')}
      />
      <TextField
        label="Опис"
        multiline
        minRows={3}
        value={values.details}
        onChange={(event) => set('details', event.target.value)}
        {...message(errors, 'details')}
      />
      <FormControlLabel
        control={
          <Switch
            checked={values.isImportant}
            onChange={(event) => set('isImportant', event.target.checked)}
          />
        }
        label="Важливо"
      />
    </>
  );
}
