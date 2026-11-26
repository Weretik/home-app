import { formatDay, formatSignedAmount } from '../data/formatting';
import { personLabels } from '../data/labels';
import type { Month } from '../data/month';
import type { SavingsMovementItem } from '../data/savings-api';
import { useDeleteSavingsMovement } from '../data/writes-api';
import { DeleteEntryDialog } from '../entry-dialogs/DeleteEntryDialog';
import type { EntryEditor } from '../entry-editing/use-entry-editor';
import { SavingsDialog } from './SavingsDialog';

/** The open dialog of the savings page: add, edit or delete a movement. */
export function SavingsEditor({
  editor,
  month,
}: {
  editor: EntryEditor<SavingsMovementItem>;
  month: Month;
}) {
  const remove = useDeleteSavingsMovement();
  const { state, close } = editor;
  if (!state) return null;
  if (state.mode === 'delete')
    return (
      <DeleteEntryDialog
        title="Удалить движение?"
        summary={`${formatSignedAmount(state.item.amount)} · ${personLabels[state.item.person]} · ${formatDay(state.item.date)}`}
        id={state.item.id}
        remove={remove}
        onClose={close}
      />
    );
  return (
    <SavingsDialog
      item={state.mode === 'edit' ? state.item : undefined}
      month={month}
      onClose={close}
    />
  );
}
