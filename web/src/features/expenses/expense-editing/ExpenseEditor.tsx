import type { ExpenseItem } from '../data/expenses-api';
import { formatAmount, formatDay } from '../data/formatting';
import type { Month } from '../data/month';
import { useDeleteExpense } from '../data/writes-api';
import { DeleteEntryDialog } from '../entry-dialogs/DeleteEntryDialog';
import type { EntryEditor } from '../entry-editing/use-entry-editor';
import { ExpenseDialog } from './ExpenseDialog';

/** The open dialog of the expense list: add, edit or delete. */
export function ExpenseEditor({
  editor,
  month,
}: {
  editor: EntryEditor<ExpenseItem>;
  month: Month;
}) {
  const remove = useDeleteExpense();
  const { state, close } = editor;
  if (!state) return null;
  if (state.mode === 'delete')
    return (
      <DeleteEntryDialog
        title="Удалить расход?"
        summary={`${state.item.categoryName} · ${formatAmount(state.item.amount)} · ${formatDay(state.item.date)}`}
        id={state.item.id}
        remove={remove}
        onClose={close}
      />
    );
  return (
    <ExpenseDialog
      item={state.mode === 'edit' ? state.item : undefined}
      month={month}
      onClose={close}
    />
  );
}
