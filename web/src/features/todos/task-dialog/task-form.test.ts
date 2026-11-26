import { describe, expect, it } from 'vitest';
import type { TodoTaskItem } from '../data/todos-api';
import {
  isValid,
  quickTask,
  taskFormValues,
  toTaskCreate,
  toTaskUpdate,
  validateTask,
} from './task-form';

const item = {
  id: 't1',
  title: 'Приклеить деревяшки',
  notes: null,
  assignee: 'Maria',
  author: 'Vitaliy',
  status: 'InProgress',
  priority: 'High',
  dueDate: '2026-11-30',
  categoryId: 'c-home',
  categoryName: 'Дом',
  categoryEmoji: '🏠',
  createdAtUtc: '2026-11-20T10:00:00Z',
  updatedAtUtc: '2026-11-20T10:00:00Z',
  completedAtUtc: null,
  steps: [],
  stepsDone: 0,
  stepsTotal: 0,
  version: 3,
} as unknown as TodoTaskItem;

describe('taskFormValues', () => {
  it('starts a new task for the given assignee with the defaults', () => {
    expect(taskFormValues(undefined, 'Vitaliy')).toEqual({
      title: '',
      assignee: 'Vitaliy',
      author: '',
      status: 'Todo',
      priority: 'Normal',
      dueDate: '',
      categoryId: '',
      notes: '',
    });
  });

  it('reads an existing task, nulls as empty choices', () => {
    expect(taskFormValues(item)).toMatchObject({
      author: 'Vitaliy',
      status: 'InProgress',
      dueDate: '2026-11-30',
      notes: '',
    });
  });
});

describe('validateTask', () => {
  const values = taskFormValues(undefined);

  it('asks for a title and limits its length', () => {
    expect(validateTask({ ...values, title: '   ' }).title).toBe(
      'Введите название',
    );
    expect(validateTask({ ...values, title: 'x'.repeat(201) }).title).toMatch(
      /200/,
    );
    expect(isValid(validateTask({ ...values, title: 'Купить хлеб' }))).toBe(
      true,
    );
  });

  it('limits the notes and the date', () => {
    const errors = validateTask({
      ...values,
      title: 'a',
      notes: 'x'.repeat(4001),
      dueDate: '1999-12-31',
    });
    expect(errors.notes).toMatch(/4000/);
    expect(errors.dueDate).toBe('Дата между 2000 и 2100 годом');
  });
});

describe('requests', () => {
  it('trims the text and sends empty choices as null', () => {
    expect(
      toTaskUpdate({
        ...taskFormValues(undefined, 'Maria'),
        title: '  Позвонить  ',
        notes: '  ',
      }),
    ).toEqual({
      title: 'Позвонить',
      notes: null,
      assignee: 'Maria',
      author: null,
      status: 'Todo',
      priority: 'Normal',
      dueDate: null,
      categoryId: null,
    });
  });

  it('sends the steps of a new task with the create', () => {
    const body = toTaskCreate(taskFormValues(item), [
      'Купить клей',
      'Шлифовать',
    ]);
    expect(body).toMatchObject({
      title: 'Приклеить деревяшки',
      categoryId: 'c-home',
      steps: ['Купить клей', 'Шлифовать'],
    });
  });

  it('makes a quick task Обычно, without a date', () => {
    expect(quickTask(' Купить хлеб ', 'Shared')).toMatchObject({
      title: 'Купить хлеб',
      assignee: 'Shared',
      priority: 'Normal',
      dueDate: null,
      steps: [],
    });
  });
});
