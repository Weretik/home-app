import { describe, expect, it } from 'vitest';
import { isFiltered, quickAddKind, readFilters, toNoteQuery } from './filters';

describe('readFilters', () => {
  it('reads the filters and ignores an unknown kind', () => {
    expect(
      readFilters(new URLSearchParams('kind=likes&category=c1&q=сир')),
    ).toEqual({ kind: 'likes', category: 'c1', q: 'сир' });
    expect(readFilters(new URLSearchParams('kind=junk')).kind).toBeUndefined();
  });
});

describe('toNoteQuery', () => {
  it('sends only the tab person without filters', () => {
    expect(toNoteQuery(undefined, {})).toEqual({});
    expect(toNoteQuery('Maria', {})).toEqual({ about: 'Maria' });
  });

  it('maps the kind, the category and «Без категорії»', () => {
    expect(toNoteQuery(undefined, { kind: 'facts' })).toEqual({
      kind: ['Fact'],
    });
    expect(toNoteQuery(undefined, { category: 'c1' })).toEqual({
      categoryId: 'c1',
    });
    expect(toNoteQuery(undefined, { category: 'none' })).toEqual({
      withoutCategory: true,
    });
  });

  it('trims the search and drops a blank one', () => {
    expect(toNoteQuery(undefined, { q: ' сир ' })).toEqual({ search: 'сир' });
    expect(toNoteQuery(undefined, { q: '  ' })).toEqual({});
  });
});

describe('quickAddKind and isFiltered', () => {
  it('starts the quick add on the filtered kind, else «Любить»', () => {
    expect(quickAddKind({ kind: 'dislikes' })).toBe('Dislikes');
    expect(quickAddKind({})).toBe('Likes');
  });

  it('counts a blank search as no filter', () => {
    expect(isFiltered({ q: ' ' })).toBe(false);
    expect(isFiltered({ category: 'none' })).toBe(true);
    expect(isFiltered({ kind: 'likes' })).toBe(true);
  });
});
