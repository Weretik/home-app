import type { NoteItem } from './notes-api';

export interface NoteGroup {
  /** The category id, or `none`. */
  key: string;
  label: string;
  notes: NoteItem[];
}

/**
 * The notes by category in the API's order (R-012: categories in their order, the notes without one last), so a run
 * of notes with the same category is one group; «Без категорії» is the group without one (FR-004).
 */
export function groupByCategory(notes: readonly NoteItem[]): NoteGroup[] {
  const groups: NoteGroup[] = [];
  for (const note of notes) {
    const key = note.categoryId ?? 'none';
    let group = groups.find((g) => g.key === key);
    if (!group) {
      const name = note.categoryName ?? 'Без категорії';
      group = {
        key,
        label: note.categoryEmoji ? `${note.categoryEmoji} ${name}` : name,
        notes: [],
      };
      groups.push(group);
    }
    group.notes.push(note);
  }
  const none = groups.findIndex((g) => g.key === 'none');
  if (none >= 0) groups.push(...groups.splice(none, 1));
  return groups;
}
