import { describe, expect, it } from 'vitest';
import {
  formatNoteCountIn,
  toCategoryRequest,
  validateCategory,
} from './category-form';

describe('category form', () => {
  it('asks for a name and limits name and emoji (R-011)', () => {
    expect(validateCategory({ name: ' ', emoji: '' }).name).toBe(
      'Введіть назву',
    );
    expect(validateCategory({ name: 'x'.repeat(51), emoji: '' }).name).toMatch(
      /50/,
    );
    expect(validateCategory({ name: 'Їжа', emoji: '123456789' }).emoji).toMatch(
      /8/,
    );
  });

  it('trims and sends an empty emoji as null', () => {
    expect(toCategoryRequest({ name: ' Їжа ', emoji: ' ' })).toEqual({
      name: 'Їжа',
      emoji: null,
    });
  });

  it('counts the notes of a category', () => {
    expect(formatNoteCountIn(0)).toBe('немає нотаток');
    expect(formatNoteCountIn(1)).toBe('1 нотатка');
    expect(formatNoteCountIn(4)).toBe('4 нотатки');
  });
});
