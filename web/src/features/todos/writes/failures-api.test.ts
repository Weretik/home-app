import { describe, expect, it } from 'vitest';
import { ApiError } from '../../../api-client';
import { describeWriteError } from './failures-api';

const problem = (status: number, code?: string) =>
  new ApiError(status, { detail: 'English detail.', code });

describe('describeWriteError', () => {
  it('names a task by default', () => {
    expect(
      describeWriteError(problem(409, 'todos.changed_meanwhile')).message,
    ).toMatch(/^Справу змінили/);
    expect(
      describeWriteError(problem(404, 'todos.task_not_found')).message,
    ).toBe('Цієї справи вже немає: її видалили в іншому місці.');
  });

  it('names a category in the settings', () => {
    expect(
      describeWriteError(
        problem(409, 'todos.changed_meanwhile'),
        'зберегти',
        'category',
      ).message,
    ).toMatch(/^Категорію змінили/);
    expect(
      describeWriteError(
        problem(404, 'todos.category_not_found'),
        'видалити',
        'category',
      ).message,
    ).toBe('Цієї категорії вже немає: її видалили в іншому місці.');
  });

  it('asks for a new Idempotency-Key only after a reused one', () => {
    expect(
      describeWriteError(problem(409, 'todos.idempotency_key_reused')).renewKey,
    ).toBe(true);
    expect(
      describeWriteError(problem(409, 'todos.changed_meanwhile')).renewKey,
    ).toBeUndefined();
  });
});
