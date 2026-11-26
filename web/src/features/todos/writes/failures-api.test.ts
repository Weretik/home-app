import { describe, expect, it } from 'vitest';
import { ApiError } from '../../../api-client';
import { describeWriteError } from './failures-api';

const problem = (status: number, code?: string) =>
  new ApiError(status, { detail: 'English detail.', code });

describe('describeWriteError', () => {
  it('names a task by default', () => {
    expect(
      describeWriteError(problem(409, 'todos.changed_meanwhile')).message,
    ).toMatch(/^Задачу изменили/);
    expect(
      describeWriteError(problem(404, 'todos.task_not_found')).message,
    ).toBe('Этой задачи уже нет: её удалили в другом месте.');
  });

  it('names a category in the settings', () => {
    expect(
      describeWriteError(
        problem(409, 'todos.changed_meanwhile'),
        'сохранить',
        'category',
      ).message,
    ).toMatch(/^Категорию изменили/);
    expect(
      describeWriteError(
        problem(404, 'todos.category_not_found'),
        'удалить',
        'category',
      ).message,
    ).toBe('Этой категории уже нет: её удалили в другом месте.');
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
