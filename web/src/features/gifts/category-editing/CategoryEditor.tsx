import type { GiftCategoryItem } from '../data/gifts-api';
import { useDeleteCategory } from '../data/settings-writes-api';
import { describeWriteError } from '../data/write-errors-api';
import { DeleteDialog } from '../editing/DeleteDialog';
import type { EditorState } from '../editing/use-editor-state';
import { CategoryDialog } from './CategoryDialog';

interface CategoryEditorProps {
  state: EditorState<GiftCategoryItem>;
  onClose: () => void;
}

/** The open dialog of the «Категории» card; a category that gifts use cannot be deleted, and the dialog says why. */
export function CategoryEditor({ state, onClose }: CategoryEditorProps) {
  const remove = useDeleteCategory();
  if (!state) return null;
  if (state.mode === 'add') return <CategoryDialog onClose={onClose} />;
  if (state.mode === 'edit')
    return <CategoryDialog category={state.item} onClose={onClose} />;

  const category = state.item;
  const close = () => {
    remove.reset();
    onClose();
  };
  return (
    <DeleteDialog
      title="Удалить категорию?"
      summary={category.name}
      error={
        remove.error
          ? describeWriteError(remove.error, 'удалить').message
          : undefined
      }
      isPending={remove.isPending}
      onConfirm={() => remove.mutate(category.id, { onSuccess: close })}
      onClose={close}
    />
  );
}
