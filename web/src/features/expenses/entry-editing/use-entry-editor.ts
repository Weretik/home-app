import { useState } from 'react';

/** Which dialog of a list is open: add, edit or delete one entry, or none. */
export type EditorState<T> =
  | { mode: 'add' }
  | { mode: 'edit'; item: T }
  | { mode: 'delete'; item: T }
  | null;

export interface EntryEditor<T> {
  state: EditorState<T>;
  add: () => void;
  edit: (item: T) => void;
  remove: (item: T) => void;
  close: () => void;
}

/** The open dialog of a list page; every add opens a new form, so it gets a new Idempotency-Key. */
export function useEntryEditor<T>(): EntryEditor<T> {
  const [state, setState] = useState<EditorState<T>>(null);
  return {
    state,
    add: () => setState({ mode: 'add' }),
    edit: (item) => setState({ mode: 'edit', item }),
    remove: (item) => setState({ mode: 'delete', item }),
    close: () => setState(null),
  };
}

/** What a list row offers: change or delete its entry. */
export interface RowHandlers<T> {
  onEdit: (item: T) => void;
  onDelete: (item: T) => void;
}
