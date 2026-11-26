import { useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/api';

export type TodoCategoryItem = Schema<'TodoCategoryItem'>;

export const categoriesKey = ['todos', 'categories'] as const;

/** The family's task categories in their order, with the count of active tasks in each. */
export function useCategories() {
  return useQuery({
    queryKey: categoriesKey,
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/todos/categories', { signal })),
  });
}

/** «🏠 Дом», or the bare name without an emoji. */
export const categoryLabel = (category: {
  name: string;
  emoji?: string | null;
}) => (category.emoji ? `${category.emoji} ${category.name}` : category.name);
