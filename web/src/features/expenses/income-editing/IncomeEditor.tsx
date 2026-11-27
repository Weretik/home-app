import type { IncomeItem } from '../data/expenses-api';
import { formatAmount, formatDay } from '../data/formatting';
import { personLabels } from '../data/labels';
import type { Month } from '../data/month';
import { useDeleteIncome } from '../data/writes-api';
import { DeleteEntryDialog } from '../entry-dialogs/DeleteEntryDialog';
import type { EntryEditor } from '../entry-editing/use-entry-editor';
import { IncomeDialog } from './IncomeDialog';

/** The open dialog of the income list: add, edit or delete. */
export function IncomeEditor({
  editor,
  month,
}: {
  editor: EntryEditor<IncomeItem>;
  month: Month;
}) {
  const remove = useDeleteIncome();
  const { state, close } = editor;
  if (!state) return null;
  if (state.mode === 'delete')
    return (
      <DeleteEntryDialog
        title="Видалити дохід?"
        summary={`${personLabels[state.item.person]} · ${formatAmount(state.item.amount)} · ${formatDay(state.item.date)}`}
        id={state.item.id}
        remove={remove}
        onClose={close}
      />
    );
  return (
    <IncomeDialog
      item={state.mode === 'edit' ? state.item : undefined}
      month={month}
      onClose={close}
    />
  );
}
