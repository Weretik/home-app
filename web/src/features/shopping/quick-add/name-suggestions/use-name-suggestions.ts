import {
  useNameSuggestions,
  type NameSuggestion,
} from '../../data/shopping-api';
import { currentSegment } from './name-segment';
import { useDebouncedValue } from './use-debounced-value';

/** The API takes at most 80 characters of a query; a longer segment is a name of its own, not a start. */
const MAX_QUERY = 80;
/** The pause in typing before suggestions are asked for. */
export const SUGGEST_DELAY_MS = 200;

/**
 * Suggestions for the segment being typed (FR-201, FR-202): asked once typing pauses, none for an empty segment or a
 * failed request, the previous list kept while the next loads.
 */
export function useSuggestionsFor(text: string): NameSuggestion[] {
  const query = currentSegment(text).query.slice(0, MAX_QUERY);
  const settled = useDebouncedValue(query, SUGGEST_DELAY_MS);
  const { data, isError } = useNameSuggestions(settled);
  if (query.length === 0 || settled.length === 0 || isError) return [];
  return data ?? [];
}
