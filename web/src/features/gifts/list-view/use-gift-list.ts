import { useChangeGiftStatus } from '../data/gift-writes-api';
import {
  useGiftCategories,
  useGifts,
  type GiftItem,
  type GiftStatus,
} from '../data/gifts-api';
import { describeWriteError } from '../data/write-errors-api';
import { useEditorState } from '../editing/use-editor-state';
import type { GiftPreset } from '../gift-editing/gift-form';
import { applyFilters } from '../url-state/gift-filters';
import { useGiftFilters } from '../url-state/use-gift-filters';

/**
 * The controller of a gift tab: its list (wanted or archived), the categories, the filters from the URL (applied
 * in memory, no request), the open dialog, and the quick status change with its error.
 */
export function useGiftList(archived: boolean) {
  const gifts = useGifts(archived);
  const categories = useGiftCategories();
  const filterState = useGiftFilters(
    categories.data?.map((category) => category.id),
  );
  const editor = useEditorState<GiftItem, GiftPreset>();
  const status = useChangeGiftStatus();
  const all = gifts.data ?? [];

  return {
    gifts,
    categories: categories.data ?? [],
    filterState,
    all,
    items: applyFilters(all, filterState.filters),
    editor,
    /** A new gift starts with the filters as its recipient and category. */
    add: () =>
      editor.add({
        recipient: filterState.filters.recipient,
        categoryId: filterState.filters.categoryId,
      }),
    changeStatus: (gift: GiftItem, next: GiftStatus) =>
      status.mutate({ gift, status: next }),
    isChanging: (gift: GiftItem) =>
      status.isPending && status.variables?.gift.id === gift.id,
    statusError: status.error
      ? describeWriteError(status.error, 'изменить').message
      : undefined,
    clearStatusError: () => status.reset(),
  };
}
