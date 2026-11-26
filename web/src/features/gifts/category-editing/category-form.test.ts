import { describe, expect, it } from 'vitest';
import { toCategoryRequest, validateCategory } from './category-form';

describe('category form', () => {
  it('asks for a name up to 100 characters', () => {
    expect(validateCategory({ name: '  ' })).toEqual({
      name: 'Введите название',
    });
    expect(validateCategory({ name: 'x'.repeat(101) })).toEqual({
      name: 'Не длиннее 100 символов',
    });
    expect(validateCategory({ name: 'Книги' })).toEqual({ name: undefined });
  });

  it('trims the name', () => {
    expect(toCategoryRequest({ name: ' Книги ' })).toEqual({ name: 'Книги' });
  });
});
