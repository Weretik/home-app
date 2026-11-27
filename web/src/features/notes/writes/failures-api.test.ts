import { describe, expect, it } from 'vitest';
import { ApiError } from '../../../api-client';
import { describeWriteError, isNoteGone } from './failures-api';

const problem = (status: number, code?: string) =>
  new ApiError(status, { detail: 'English detail.', code });

describe('describeWriteError', () => {
  it('names a note by default', () => {
    expect(
      describeWriteError(problem(409, 'notes.changed_meanwhile')).message,
    ).toMatch(/^Нотатку змінили/);
    expect(
      describeWriteError(problem(404, 'notes.note_not_found')).message,
    ).toBe('Цієї нотатки вже немає: її видалили в іншому місці.');
    expect(isNoteGone(problem(404, 'notes.note_not_found'))).toBe(true);
  });

  it('names a category in the settings and puts a taken name under the field', () => {
    expect(
      describeWriteError(
        problem(404, 'notes.category_not_found'),
        'видалити',
        'category',
      ).message,
    ).toBe('Цієї категорії вже немає: її видалили в іншому місці.');
    expect(
      describeWriteError(problem(409, 'notes.category_name_taken')).fieldErrors
        .name,
    ).toEqual(['Така назва вже є']);
  });

  it('asks for a new Idempotency-Key only after a reused one', () => {
    expect(
      describeWriteError(problem(409, 'notes.idempotency_key_reused')).renewKey,
    ).toBe(true);
    expect(
      describeWriteError(problem(409, 'notes.changed_meanwhile')).renewKey,
    ).toBeUndefined();
  });
});
