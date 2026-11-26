import { describe, expect, it } from 'vitest';
import { dayHeading, groupByCompletionDay, groupByDueDay } from './day-groups';

const today = '2026-11-26'; // a Thursday

describe('dayHeading', () => {
  it('names the weekday and the month, and the year outside this one', () => {
    expect(dayHeading('2026-11-30', today)).toBe('Пн, 30 ноября');
    expect(dayHeading('2027-01-04', today)).toBe('Пн, 4 января 2027');
  });
});

describe('groupByDueDay', () => {
  it('puts overdue first, days in order, tomorrow by name and no due date last', () => {
    const items = [
      { id: 'a', dueDate: null },
      { id: 'b', dueDate: '2026-11-30' },
      { id: 'c', dueDate: '2026-11-20' },
      { id: 'd', dueDate: '2026-11-26' },
      { id: 'e', dueDate: '2026-11-27' },
      { id: 'f', dueDate: '2026-11-10' },
      { id: 'g', dueDate: '2026-11-30' },
    ];

    const groups = groupByDueDay(items, today);

    expect(groups.map((g) => g.label)).toEqual([
      'Просрочено',
      'Сегодня',
      'Завтра',
      'Пн, 30 ноября',
      'Без срока',
    ]);
    expect(groups[0]!.overdue).toBe(true);
    expect(groups[0]!.items.map((i) => i.id)).toEqual(['c', 'f']);
    expect(groups[3]!.items.map((i) => i.id)).toEqual(['b', 'g']);
  });
});

describe('groupByCompletionDay', () => {
  it('groups by the Kyiv day of completion, newest first', () => {
    const items = [
      { id: 'a', completedAtUtc: '2026-11-26T09:00:00Z' },
      // 22:30 UTC on the 25th is already the 26th in Kyiv.
      { id: 'b', completedAtUtc: '2026-11-25T22:30:00Z' },
      { id: 'c', completedAtUtc: '2026-11-25T10:00:00Z' },
      { id: 'd', completedAtUtc: '2026-11-20T10:00:00Z' },
    ];

    const groups = groupByCompletionDay(items, today);

    expect(groups.map((g) => g.label)).toEqual([
      'Сегодня',
      'Вчера',
      'Пт, 20 ноября',
    ]);
    expect(groups[0]!.items.map((i) => i.id)).toEqual(['a', 'b']);
  });
});
