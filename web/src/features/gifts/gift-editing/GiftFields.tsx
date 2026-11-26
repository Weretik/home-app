import MenuItem from '@mui/material/MenuItem';
import TextField from '@mui/material/TextField';
import type {
  GiftCategoryItem,
  GiftPriority,
  Recipient,
} from '../data/gifts-api';
import {
  priorities,
  priorityLabels,
  recipientLabels,
  recipients,
} from '../display/labels';
import type { FieldErrors } from '../editing/field-checks';
import { PriceField } from '../variant-editing/PriceField';
import type { GiftFormValues } from './gift-form';

interface GiftFieldsProps {
  values: GiftFormValues;
  errors: FieldErrors;
  categories: GiftCategoryItem[];
  set: <K extends keyof GiftFormValues>(
    name: K,
    value: GiftFormValues[K],
  ) => void;
}

/** The main facts of a gift: for whom, what, category, priority, price and link. */
export function GiftFields({
  values,
  errors,
  categories,
  set,
}: GiftFieldsProps) {
  const field = (name: keyof GiftFormValues) => ({
    error: Boolean(errors[name]),
    helperText: errors[name],
  });

  return (
    <>
      <TextField
        select
        label="Для кого"
        value={values.recipient}
        onChange={(event) => set('recipient', event.target.value as Recipient)}
        {...field('recipient')}
      >
        {recipients.map((recipient) => (
          <MenuItem key={recipient} value={recipient}>
            {recipientLabels[recipient]}
          </MenuItem>
        ))}
      </TextField>
      <TextField
        label="Название"
        value={values.name}
        onChange={(event) => set('name', event.target.value)}
        {...field('name')}
      />
      <TextField
        select
        label="Категория"
        value={values.categoryId}
        onChange={(event) => set('categoryId', event.target.value)}
        {...field('categoryId')}
      >
        {categories.map((category) => (
          <MenuItem key={category.id} value={category.id}>
            {category.name}
          </MenuItem>
        ))}
      </TextField>
      <TextField
        select
        label="Приоритет"
        value={values.priority}
        onChange={(event) =>
          set('priority', event.target.value as GiftPriority)
        }
      >
        {priorities.map((priority) => (
          <MenuItem key={priority} value={priority}>
            {priorityLabels[priority]}
          </MenuItem>
        ))}
      </TextField>
      <PriceField
        label="Цена (если известна)"
        value={values.price}
        error={errors.price}
        onChange={(price) => set('price', price)}
      />
      <TextField
        label="Ссылка"
        value={values.link}
        onChange={(event) => set('link', event.target.value)}
        slotProps={{ htmlInput: { inputMode: 'url' } }}
        {...field('link')}
      />
    </>
  );
}
