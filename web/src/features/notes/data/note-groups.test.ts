import { expect, test } from 'vitest';
import { groupByCategory } from './note-groups';
import type { NoteItem } from './notes-api';

const note = (id: string, category?: [string, string, string | null]) =>
  ({
    id,
    categoryId: category?.[0] ?? null,
    categoryName: category?.[1] ?? null,
    categoryEmoji: category?.[2] ?? null,
  }) as NoteItem;

test('groups the notes by category in the API order, «Без категорії» last (FR-004)', () => {
  const gifts: [string, string, string | null] = ['c1', 'Подарунки', '🎁'];
  const food: [string, string, string | null] = ['c2', 'Їжа', null];
  const groups = groupByCategory([
    note('a', gifts),
    note('b', gifts),
    note('n'),
    note('c', food),
  ]);

  expect(groups.map((g) => [g.key, g.label, g.notes.map((n) => n.id)])).toEqual(
    [
      ['c1', '🎁 Подарунки', ['a', 'b']],
      ['c2', 'Їжа', ['c']],
      ['none', 'Без категорії', ['n']],
    ],
  );
  expect(groupByCategory([])).toEqual([]);
});
