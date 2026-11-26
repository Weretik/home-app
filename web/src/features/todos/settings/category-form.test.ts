import { describe, expect, it } from 'vitest';
import {
  formatActiveCount,
  toCategoryRequest,
  validateCategory,
} from './category-form';

describe('category form', () => {
  it('asks for a name and limits name and emoji', () => {
    expect(validateCategory({ name: ' ', emoji: '' }).name).toBe(
      'Введите название',
    );
    expect(validateCategory({ name: 'x'.repeat(51), emoji: '' }).name).toMatch(
      /50/,
    );
    expect(validateCategory({ name: 'Дом', emoji: '123456789' }).emoji).toMatch(
      /8/,
    );
  });

  it('trims and sends an empty emoji as null', () => {
    expect(toCategoryRequest({ name: ' Сад ', emoji: ' ' })).toEqual({
      name: 'Сад',
      emoji: null,
    });
  });

  it('counts active tasks in Russian', () => {
    expect(formatActiveCount(0)).toBe('нет активных задач');
    expect(formatActiveCount(1)).toBe('1 активная задача');
    expect(formatActiveCount(3)).toBe('3 активные задачи');
    expect(formatActiveCount(5)).toBe('5 активных задач');
  });
});
