import type { NoteCategoryItem } from '../data/categories-api';
import type { NoteCategoryRequest } from '../writes/category-writes-api';
import { formatNoteCount } from '../data/labels';

/** The API limits (R-011). */
export const MAX_NAME = 50;
export const MAX_EMOJI = 8;

export interface CategoryFormValues {
  name: string;
  emoji: string;
}

export const categoryFormValues = (
  item: NoteCategoryItem | undefined,
): CategoryFormValues => ({ name: item?.name ?? '', emoji: item?.emoji ?? '' });

export function validateCategory(
  values: CategoryFormValues,
): Record<string, string | undefined> {
  const name = values.name.trim();
  return {
    name:
      name === ''
        ? 'Введіть назву'
        : name.length > MAX_NAME
          ? `Назва не довша за ${MAX_NAME} символів`
          : undefined,
    emoji:
      values.emoji.trim().length > MAX_EMOJI
        ? `Не більше ${MAX_EMOJI} символів`
        : undefined,
  };
}

export function toCategoryRequest(
  values: CategoryFormValues,
): NoteCategoryRequest {
  const emoji = values.emoji.trim();
  return { name: values.name.trim(), emoji: emoji === '' ? null : emoji };
}

/** «немає нотаток», «1 нотатка», «3 нотатки», «5 нотаток». */
export function formatNoteCountIn(count: number): string {
  return count === 0 ? 'немає нотаток' : formatNoteCount(count);
}
