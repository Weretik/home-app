import { useState } from 'react';

/** Which dialog of a list is open: add (with preset values), edit or delete one item, or none. */
export type EditorState<T, Preset = undefined> =
  | { mode: 'add'; preset?: Preset }
  | { mode: 'edit'; item: T }
  | { mode: 'delete'; item: T }
  | null;

export interface Editor<T, Preset = undefined> {
  state: EditorState<T, Preset>;
  add: (preset?: Preset) => void;
  edit: (item: T) => void;
  remove: (item: T) => void;
  close: () => void;
}

/** The open dialog of a page or card; every add mounts a new form, so it gets a new Idempotency-Key. */
export function useEditorState<T, Preset = undefined>(): Editor<T, Preset> {
  const [state, setState] = useState<EditorState<T, Preset>>(null);
  return {
    state,
    add: (preset) => setState({ mode: 'add', preset }),
    edit: (item) => setState({ mode: 'edit', item }),
    remove: (item) => setState({ mode: 'delete', item }),
    close: () => setState(null),
  };
}
