import { describe, expect, it } from 'vitest';
import { isFiltered, quickAddAssignee, toTaskQuery } from './filters';

describe('toTaskQuery', () => {
  it('sends nothing without filters', () => {
    expect(toTaskQuery({})).toEqual({});
  });

  it('brings the shared tasks with a person, only shared ones with «Спільне»', () => {
    expect(toTaskQuery({ who: 'maria' }).assignee).toEqual(['Maria', 'Shared']);
    expect(toTaskQuery({ who: 'vitaliy' }).assignee).toEqual([
      'Vitaliy',
      'Shared',
    ]);
    expect(toTaskQuery({ who: 'shared' }).assignee).toEqual(['Shared']);
  });

  it('maps the category and «Без категорії»', () => {
    expect(toTaskQuery({ category: 'c1' })).toEqual({ categoryId: 'c1' });
    expect(toTaskQuery({ category: 'none' })).toEqual({
      withoutCategory: true,
    });
  });

  it('adds the date filter of the list it is on', () => {
    expect(toTaskQuery({ when: 'today' }, 'active', '2026-11-26')).toEqual({
      dueFrom: '2026-11-26',
      dueTo: '2026-11-26',
    });
    expect(toTaskQuery({ when: 'today' }, 'archive', '2026-11-26')).toEqual({
      completedFrom: '2026-11-26',
      completedTo: '2026-11-26',
    });
  });

  it('trims the search and drops a blank one', () => {
    expect(toTaskQuery({ q: '  балкон ' })).toEqual({ search: 'балкон' });
    expect(toTaskQuery({ q: '   ' })).toEqual({});
  });
});

describe('quickAddAssignee and isFiltered', () => {
  it('gives a quick-added task the chosen person, else «Спільне»', () => {
    expect(quickAddAssignee('maria')).toBe('Maria');
    expect(quickAddAssignee(undefined)).toBe('Shared');
  });

  it('counts a blank search as no filter', () => {
    expect(isFiltered({ q: ' ' })).toBe(false);
    expect(isFiltered({ category: 'none' })).toBe(true);
    expect(isFiltered({ when: 'week' })).toBe(true);
  });
});
