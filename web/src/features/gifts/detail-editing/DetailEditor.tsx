import type { GiftDetailItem, Recipient } from '../data/gifts-api';
import { useDeleteDetail } from '../data/settings-writes-api';
import { describeWriteError } from '../data/write-errors-api';
import { recipientLabels } from '../display/labels';
import { DeleteDialog } from '../editing/DeleteDialog';
import type { EditorState } from '../editing/use-editor-state';
import { DetailDialog } from './DetailDialog';

interface DetailEditorProps {
  state: EditorState<GiftDetailItem, Recipient>;
  onClose: () => void;
}

/** The open dialog of the «Нюансы» card: add, edit or delete one. */
export function DetailEditor({ state, onClose }: DetailEditorProps) {
  const remove = useDeleteDetail();
  if (!state) return null;
  if (state.mode === 'add')
    return <DetailDialog preset={state.preset} onClose={onClose} />;
  if (state.mode === 'edit')
    return <DetailDialog detail={state.item} onClose={onClose} />;

  const detail = state.item;
  const close = () => {
    remove.reset();
    onClose();
  };
  return (
    <DeleteDialog
      title="Удалить нюанс?"
      summary={`${recipientLabels[detail.recipient]} · ${detail.label} — ${detail.value}`}
      error={
        remove.error
          ? describeWriteError(remove.error, 'удалить').message
          : undefined
      }
      isPending={remove.isPending}
      onConfirm={() => remove.mutate(detail.id, { onSuccess: close })}
      onClose={close}
    />
  );
}
