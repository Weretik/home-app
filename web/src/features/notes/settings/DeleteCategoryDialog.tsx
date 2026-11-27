import { categoryLabel, type NoteCategoryItem } from '../data/categories-api';
import { formatNoteCount } from '../data/labels';
import { ConfirmDeleteDialog } from '../feedback/ConfirmDeleteDialog';
import { describeWriteError } from '../writes/failures-api';
import { useDeleteCategory } from '../writes/category-writes-api';

/** Asks before a category is deleted and says how many notes lose it (FR-006, R-006). */
export function DeleteCategoryDialog({
  item,
  onClose,
}: {
  item: NoteCategoryItem;
  onClose: () => void;
}) {
  const remove = useDeleteCategory();
  const warning =
    item.noteCount > 0
      ? `${formatNoteCount(item.noteCount)} залишаться без категорії. Самі нотатки не видаляться.`
      : 'Нотаток у ній немає.';

  return (
    <ConfirmDeleteDialog
      title="Видалити категорію?"
      summary={categoryLabel(item)}
      warning={warning}
      error={
        remove.error
          ? describeWriteError(remove.error, 'видалити', 'category').message
          : undefined
      }
      isPending={remove.isPending}
      onConfirm={() =>
        remove.mutate(
          { id: item.id, version: item.version },
          { onSuccess: onClose },
        )
      }
      onClose={onClose}
    />
  );
}
