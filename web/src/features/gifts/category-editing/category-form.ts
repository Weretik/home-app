import type { GiftCategoryRequest } from '../data/settings-writes-api';
import { checkLength, type FieldErrors } from '../editing/field-checks';

export interface CategoryFormValues {
  name: string;
}

export const validateCategory = (values: CategoryFormValues): FieldErrors => ({
  name: checkLength(values.name, 100, 'Введіть назву'),
});

export const toCategoryRequest = (
  values: CategoryFormValues,
): GiftCategoryRequest => ({
  name: values.name.trim(),
});
