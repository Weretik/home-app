import { categoryLabel, type TodoCategoryItem } from '../data/categories-api';
import { formatTaskCount } from '../data/labels';
import { ConfirmDeleteDialog } from '../feedback/ConfirmDeleteDialog';
import { describeWriteError } from '../writes/failures-api';
import { useDeleteCategory } from '../writes/category-writes-api';

/** Asks before a category is deleted and says how many tasks lose it (FR-007, R-008). */
export function DeleteCategoryDialog({
  item,
  onClose,
}: {
  item: TodoCategoryItem;
  onClose: () => void;
}) {
  const remove = useDeleteCategory();
  const warning =
    item.activeCount > 0
      ? `${formatTaskCount(item.activeCount)} залишаться без категорії. Самі справи не видаляться.`
      : 'Активних справ у ній немає. Справи з архіву залишаться без категорії.';

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
