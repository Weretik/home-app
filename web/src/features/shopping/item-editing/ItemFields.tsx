import FormControlLabel from '@mui/material/FormControlLabel';
import MenuItem from '@mui/material/MenuItem';
import Switch from '@mui/material/Switch';
import TextField from '@mui/material/TextField';
import type { Department } from '../department-settings/departments-api';
import { NOTE_MAX, type ItemErrors, type ItemValues } from './item-form';

interface ItemFieldsProps {
  values: ItemValues;
  errors: ItemErrors;
  departments: Department[];
  onChange: (values: ItemValues) => void;
}

/** Name, quantity, note, department and «Терміново» of an item, one per row. */
export function ItemFields({
  values,
  errors,
  departments,
  onChange,
}: ItemFieldsProps) {
  const set =
    <K extends keyof ItemValues>(key: K) =>
    (value: ItemValues[K]) =>
      onChange({ ...values, [key]: value });

  return (
    <>
      <TextField
        label="Назва"
        required
        value={values.name}
        onChange={(e) => set('name')(e.target.value)}
        error={!!errors.name}
        helperText={errors.name}
      />
      <TextField
        label="Кількість"
        placeholder="2, 300 г, 1 л"
        value={values.quantity}
        onChange={(e) => set('quantity')(e.target.value)}
        error={!!errors.quantity}
        helperText={errors.quantity}
      />
      <TextField
        label="Нотатка"
        multiline
        minRows={2}
        value={values.note}
        onChange={(e) => set('note')(e.target.value)}
        error={!!errors.note}
        helperText={errors.note ?? `До ${NOTE_MAX} символів`}
      />
      <TextField
        select
        label="Відділ"
        required
        value={
          departments.some((d) => d.id === values.departmentId)
            ? values.departmentId
            : ''
        }
        onChange={(e) => set('departmentId')(e.target.value)}
        error={!!errors.departmentId}
        helperText={
          errors.departmentId ?? 'Новий відділ запам’ятається для цього товару'
        }
      >
        {departments.map((department) => (
          <MenuItem key={department.id} value={department.id}>
            {department.icon} {department.name}
          </MenuItem>
        ))}
      </TextField>
      <FormControlLabel
        control={
          <Switch
            checked={values.urgent}
            onChange={(e) => set('urgent')(e.target.checked)}
            color="secondary"
          />
        }
        label="Терміново"
      />
    </>
  );
}
