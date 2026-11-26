import { describe, expect, it } from 'vitest';
import { ApiError } from '../../../api-client';
import { describeWriteError, isStale } from './write-errors-api';

const problem = (
  status: number,
  code?: string,
  errors?: Record<string, string[]>,
) => new ApiError(status, { detail: 'Server says no.', code, errors });

describe('describeWriteError', () => {
  it('says there is no connection when the request never got an answer', () => {
    expect(describeWriteError(new TypeError('Failed to fetch')).message).toBe(
      'Нет связи с сервером. Проверьте интернет и попробуйте снова.',
    );
  });

  it('explains a change made elsewhere, also for 412', () => {
    const text =
      'Подарок или запись изменили в другом месте — список обновлён, откройте снова.';
    expect(
      describeWriteError(problem(409, 'gifts.changed_meanwhile')).message,
    ).toBe(text);
    expect(describeWriteError(problem(412)).message).toBe(text);
  });

  it('says the item is gone after a 404', () => {
    expect(
      describeWriteError(problem(404, 'gifts.gift_not_found')).message,
    ).toBe('Этого уже нет: его удалили в другом месте.');
  });

  it('renews the key after it was used for other data', () => {
    const failure = describeWriteError(
      problem(409, 'gifts.idempotency_key_reused'),
    );
    expect(failure.renewKey).toBe(true);
  });

  it('explains a category that gifts still use', () => {
    expect(
      describeWriteError(problem(409, 'gifts.category_in_use'), 'удалить')
        .message,
    ).toBe('Эта категория есть у подарков — сначала перенесите их в другую.');
  });

  it('puts a taken category name under the name field', () => {
    const failure = describeWriteError(
      problem(409, 'gifts.category_name_taken'),
    );
    expect(failure.fieldErrors).toEqual({ name: ['Такая категория уже есть'] });
  });

  it('asks to wait after too many writes', () => {
    expect(describeWriteError(problem(429)).message).toBe(
      'Слишком много изменений подряд. Подождите минуту и повторите.',
    );
  });

  it('passes field errors of a 400 through', () => {
    const failure = describeWriteError(
      problem(400, 'validation', { link: ['Bad link'] }),
    );
    expect(failure.message).toBe('Проверьте поля: Server says no.');
    expect(failure.fieldErrors).toEqual({ link: ['Bad link'] });
  });

  it('names the failed action otherwise', () => {
    expect(describeWriteError(problem(500), 'удалить').message).toBe(
      'Не удалось удалить: Server says no.',
    );
  });
});

describe('isStale', () => {
  it('is true for 404, 412 and a change made elsewhere', () => {
    expect(isStale(problem(404))).toBe(true);
    expect(isStale(problem(412))).toBe(true);
    expect(isStale(problem(409, 'gifts.changed_meanwhile'))).toBe(true);
    expect(isStale(problem(409, 'gifts.category_in_use'))).toBe(false);
    expect(isStale(new Error('x'))).toBe(false);
  });
});
