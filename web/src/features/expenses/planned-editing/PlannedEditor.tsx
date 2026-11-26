import type { Month } from '../data/month';
import { DeleteEntryDialog } from '../entry-dialogs/DeleteEntryDialog';
import type { EntryEditor } from '../entry-editing/use-entry-editor';
import type { PlannedExpenseItem } from '../planned-data/planned-api';
import { useDeletePlannedExpense } from '../planned-data/planned-writes-api';
import { PlannedDialog } from './PlannedDialog';
import { RecordDialog } from './RecordDialog';

interface PlannedEditorProps {
  editor: EntryEditor<PlannedExpenseItem>;
  /** The plan «Записать как расход» is open for; null when it is closed. */
  recording: PlannedExpenseItem | null;
  onRecordClose: () => void;
  /** The open month, where a new plan goes. */
  month: Month;
  /** A plan in one line, for the delete and record dialogs. */
  describe: (item: PlannedExpenseItem) => string;
}

/** The open dialog of the «Прогноз» list: add, edit, delete, or record a plan as an expense. */
export function PlannedEditor({
  editor,
  recording,
  onRecordClose,
  month,
  describe,
}: PlannedEditorProps) {
  const remove = useDeletePlannedExpense();
  const { state, close } = editor;
  if (recording)
    return (
      <RecordDialog
        item={recording}
        summary={describe(recording)}
        onClose={onRecordClose}
      />
    );
  if (!state) return null;
  if (state.mode === 'delete')
    return (
      <DeleteEntryDialog
        title="Удалить план?"
        summary={describe(state.item)}
        id={state.item.id}
        remove={remove}
        onClose={close}
      />
    );
  return (
    <PlannedDialog
      item={state.mode === 'edit' ? state.item : undefined}
      month={month}
      onClose={close}
    />
  );
}
