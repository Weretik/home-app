import type { TodoCategoryItem } from '../data/categories-api';
import type { TodoCategoryRequest } from '../writes/category-writes-api';
import { locale } from '../../../platform/client/locale';

/** The API limits (R-011). */
export const MAX_NAME = 50;
export const MAX_EMOJI = 8;

export interface CategoryFormValues {
  name: string;
  emoji: string;
}

export const categoryFormValues = (
  item: TodoCategoryItem | undefined,
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
): TodoCategoryRequest {
  const emoji = values.emoji.trim();
  return { name: values.name.trim(), emoji: emoji === '' ? null : emoji };
}

/** «немає активних справ», «1 активна справа», «3 активні справи», «5 активних справ». */
export function formatActiveCount(count: number): string {
  if (count === 0) return 'немає активних справ';
  const forms = {
    one: 'активна справа',
    few: 'активні справи',
    many: 'активних справ',
  } as const;
  const rule = new Intl.PluralRules(locale).select(count);
  return `${count} ${forms[rule as keyof typeof forms] ?? forms.many}`;
}
