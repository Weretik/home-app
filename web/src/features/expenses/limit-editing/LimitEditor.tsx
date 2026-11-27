import type { SpendingLimitItem } from '../budget-data/budget-api';
import { useDeleteSpendingLimit } from '../budget-data/budget-writes-api';
import { formatAmount } from '../data/formatting';
import { limitPeriodLabels } from '../data/labels';
import { DeleteEntryDialog } from '../entry-dialogs/DeleteEntryDialog';
import type { EntryEditor } from '../entry-editing/use-entry-editor';
import { LimitDialog } from './LimitDialog';

/** The open dialog of the limits list: add, edit or delete. */
export function LimitEditor({
  editor,
}: {
  editor: EntryEditor<SpendingLimitItem>;
}) {
  const remove = useDeleteSpendingLimit();
  const { state, close } = editor;
  if (!state) return null;
  if (state.mode === 'delete')
    return (
      <DeleteEntryDialog
        title="Видалити ліміт?"
        summary={`${state.item.name} · ${formatAmount(state.item.amount)} ${limitPeriodLabels[state.item.period]}`}
        id={state.item.id}
        remove={remove}
        onClose={close}
      />
    );
  return (
    <LimitDialog
      item={state.mode === 'edit' ? state.item : undefined}
      onClose={close}
    />
  );
}
