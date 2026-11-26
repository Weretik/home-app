import type { TodoCategoryItem } from '../data/categories-api';
import type { TodoCategoryRequest } from '../writes/category-writes-api';

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
        ? 'Введите название'
        : name.length > MAX_NAME
          ? `Название не длиннее ${MAX_NAME} символов`
          : undefined,
    emoji:
      values.emoji.trim().length > MAX_EMOJI
        ? `Не больше ${MAX_EMOJI} символов`
        : undefined,
  };
}

export function toCategoryRequest(
  values: CategoryFormValues,
): TodoCategoryRequest {
  const emoji = values.emoji.trim();
  return { name: values.name.trim(), emoji: emoji === '' ? null : emoji };
}

/** «нет активных задач», «1 активная задача», «3 активные задачи», «5 активных задач». */
export function formatActiveCount(count: number): string {
  if (count === 0) return 'нет активных задач';
  const forms = {
    one: 'активная задача',
    few: 'активные задачи',
    many: 'активных задач',
  } as const;
  const rule = new Intl.PluralRules('ru-RU').select(count);
  return `${count} ${forms[rule as keyof typeof forms] ?? forms.many}`;
}
