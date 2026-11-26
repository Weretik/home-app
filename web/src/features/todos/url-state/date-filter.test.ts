import { describe, expect, it } from 'vitest';
import { dateQuery, whenFits } from './date-filter';

const today = '2026-11-26';

describe('dateQuery on the active list', () => {
  it.each([
    ['overdue', { dueTo: '2026-11-25' }],
    ['today', { dueFrom: today, dueTo: today }],
    ['tomorrow', { dueFrom: '2026-11-27', dueTo: '2026-11-27' }],
    ['week', { dueTo: '2026-12-02' }],
    ['month', { dueTo: '2026-12-25' }],
    ['none', { withoutDueDate: true }],
  ] as const)('%s reads the due date', (when, query) => {
    expect(dateQuery('active', { when }, today)).toEqual(query);
  });

  it('takes a period in either order and ignores a broken day', () => {
    expect(
      dateQuery(
        'active',
        { when: 'period', from: '2026-12-05', to: '2026-12-01' },
        today,
      ),
    ).toEqual({ dueFrom: '2026-12-01', dueTo: '2026-12-05' });
    expect(
      dateQuery(
        'active',
        { when: 'period', from: 'x', to: '2026-12-01' },
        today,
      ),
    ).toEqual({ dueTo: '2026-12-01' });
  });
});

describe('dateQuery in the archive', () => {
  it('reads the completion day and looks back', () => {
    expect(dateQuery('archive', { when: 'yesterday' }, today)).toEqual({
      completedFrom: '2026-11-25',
      completedTo: '2026-11-25',
    });
    expect(dateQuery('archive', { when: 'week' }, today)).toEqual({
      completedFrom: '2026-11-20',
      completedTo: today,
    });
  });

  it('ignores a choice the archive does not offer', () => {
    expect(whenFits('archive', 'none')).toBe(false);
    expect(dateQuery('archive', { when: 'overdue' }, today)).toEqual({});
  });
});
