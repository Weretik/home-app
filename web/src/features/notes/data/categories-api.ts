import { useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/client/api';
import { moduleKeys } from '../../../platform/client/query-client';

export type NoteCategoryItem = Schema<'NoteCategoryItem'>;

export const categoriesKey = [...moduleKeys.notes, 'categories'] as const;

/** The note categories in their order, with how many notes each holds. */
export function useCategories() {
  return useQuery({
    queryKey: categoriesKey,
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/notes/categories', { signal })),
  });
}

/** «🎁 Подарунки», or the bare name without an emoji. */
export const categoryLabel = (category: {
  name: string;
  emoji?: string | null;
}) => (category.emoji ? `${category.emoji} ${category.name}` : category.name);
