import { useQuery, queryOptions } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/client/api';
import { moduleKeys } from '../../../platform/client/query-client';

export type TodoCategoryItem = Schema<'TodoCategoryItem'>;

export const categoriesKey = [...moduleKeys.todos, 'categories'] as const;

/** The query of `useCategories`, also used to load it ahead for offline use (web ADR 0022). */
export const categoriesQuery = () =>
  queryOptions({
    queryKey: categoriesKey,
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/todos/categories', { signal })),
  });

/** The family's task categories in their order, with the count of active tasks in each. */
export function useCategories() {
  return useQuery({ ...categoriesQuery() });
}

/** «🏠 Дім», or the bare name without an emoji. */
export const categoryLabel = (category: {
  name: string;
  emoji?: string | null;
}) => (category.emoji ? `${category.emoji} ${category.name}` : category.name);
