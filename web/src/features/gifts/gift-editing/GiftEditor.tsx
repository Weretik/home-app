import type { GiftCategoryItem, GiftItem } from '../data/gifts-api';
import { useDeleteGift } from '../data/gift-writes-api';
import { describeWriteError } from '../data/write-errors-api';
import { recipientLabels } from '../display/labels';
import { DeleteDialog } from '../editing/DeleteDialog';
import type { EditorState } from '../editing/use-editor-state';
import type { GiftPreset } from './gift-form';
import { GiftDialog } from './GiftDialog';

interface GiftEditorProps {
  state: EditorState<GiftItem, GiftPreset>;
  categories: GiftCategoryItem[];
  onClose: () => void;
}

/** The open dialog of a gift list: add, edit or delete one gift. */
export function GiftEditor({ state, categories, onClose }: GiftEditorProps) {
  const remove = useDeleteGift();
  if (!state) return null;
  if (state.mode === 'add')
    return (
      <GiftDialog
        preset={state.preset}
        categories={categories}
        onClose={onClose}
      />
    );
  if (state.mode === 'edit')
    return (
      <GiftDialog gift={state.item} categories={categories} onClose={onClose} />
    );

  const gift = state.item;
  const close = () => {
    remove.reset();
    onClose();
  };
  return (
    <DeleteDialog
      title="Видалити подарунок?"
      summary={`${gift.name} · ${recipientLabels[gift.recipient]}`}
      error={
        remove.error
          ? describeWriteError(remove.error, 'видалити').message
          : undefined
      }
      isPending={remove.isPending}
      onConfirm={() => remove.mutate(gift.id, { onSuccess: close })}
      onClose={close}
    />
  );
}
