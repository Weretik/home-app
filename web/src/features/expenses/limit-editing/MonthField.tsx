import TextField from '@mui/material/TextField';
import type { LimitPeriod } from '../budget-data/budget-api';
import { fieldMessage, type FieldErrors } from '../entry-editing/entry-fields';

interface MonthFieldProps {
  /** «YYYY-MM», as the native month input holds it. */
  value: string;
  onChange: (value: string) => void;
  errors: FieldErrors;
  /** The contract's field name, whose message shows under the field. */
  name: string;
  /** A quarterly limit starts its amount at the quarter holding the month. */
  period?: LimitPeriod;
  /** «Діє з» unless the field needs to say whose month it is («Ліміт діє з» beside other amounts). */
  label?: string;
}

const hints: Record<LimitPeriod, string> = {
  Month: 'Попередні місяці зберігають свій ліміт',
  Quarter:
    'Сума діє з початку кварталу, у який входить цей місяць; попередні квартали зберігають свій ліміт',
};

/** «Діє з»: the month a new limit amount applies from (web ADR 0018). */
export function MonthField({
  value,
  onChange,
  errors,
  name,
  period = 'Month',
  label = 'Діє з',
}: MonthFieldProps) {
  const message = fieldMessage(errors, name);
  return (
    <TextField
      label={label}
      type="month"
      required
      value={value}
      onChange={(event) => onChange(event.target.value)}
      error={message.error}
      helperText={message.helperText ?? hints[period]}
      slotProps={{
        inputLabel: { shrink: true },
        // the placeholder shows only where a browser has no month picker and takes text
        htmlInput: { min: '2000-01', max: '2100-12', placeholder: 'РРРР-ММ' },
      }}
    />
  );
}
