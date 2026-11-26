import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';
import type { Person } from '../data/expenses-api';
import { personLabels } from '../data/labels';
import { fieldMessage, type FieldErrors } from '../entry-editing/entry-fields';

const people = Object.keys(personLabels) as Person[];

/** «Кто»: one of the two people. */
export function PersonField({
  value,
  onChange,
  errors,
}: {
  value: Person;
  onChange: (value: Person) => void;
  errors: FieldErrors;
}) {
  return (
    <TextField
      select
      label="Кто"
      value={value}
      onChange={(event) => onChange(event.target.value as Person)}
      {...fieldMessage(errors, 'person')}
    >
      {people.map((person) => (
        <MenuItem key={person} value={person}>
          {personLabels[person]}
        </MenuItem>
      ))}
    </TextField>
  );
}
