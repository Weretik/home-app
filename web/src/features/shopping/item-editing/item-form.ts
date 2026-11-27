import type { UpdateItemRequest } from '../data/item-writes-api';
import type { ShoppingItem } from '../data/shopping-api';

/** The edit form of an item as typed. */
export interface ItemValues {
  name: string;
  quantity: string;
  note: string;
  departmentId: string;
  urgent: boolean;
}

export type ItemErrors = Partial<Record<keyof ItemValues, string>>;

export const NAME_MAX = 80;
export const QUANTITY_MAX = 30;
export const NOTE_MAX = 200;

export const itemValues = (item: ShoppingItem): ItemValues => ({
  name: item.name,
  quantity: item.quantity ?? '',
  note: item.note ?? '',
  departmentId: item.departmentId,
  urgent: item.urgent,
});

/** The messages under the fields; empty when the form can be sent (the API checks the same limits). */
export function checkItem(values: ItemValues): ItemErrors {
  const errors: ItemErrors = {};
  const name = values.name.trim();
  if (!name) errors.name = 'Введіть назву';
  else if (name.length > NAME_MAX)
    errors.name = `Не довше ${NAME_MAX} символів`;
  if (values.quantity.trim().length > QUANTITY_MAX)
    errors.quantity = `Не довше ${QUANTITY_MAX} символів`;
  if (values.note.trim().length > NOTE_MAX)
    errors.note = `Не довше ${NOTE_MAX} символів`;
  if (!values.departmentId) errors.departmentId = 'Виберіть відділ';
  return errors;
}

/** The request of a checked form: trimmed text, empty quantity and note as null. */
export function itemRequest(values: ItemValues): UpdateItemRequest {
  const quantity = values.quantity.trim();
  const note = values.note.trim();
  return {
    name: values.name.trim(),
    quantity: quantity || null,
    note: note || null,
    departmentId: values.departmentId,
    urgent: values.urgent,
  };
}
