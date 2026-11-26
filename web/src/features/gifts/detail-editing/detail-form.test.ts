import { describe, expect, it } from 'vitest';
import {
  emptyDetail,
  fromDetail,
  toDetailRequest,
  validateDetail,
} from './detail-form';

describe('detail form', () => {
  it('starts with the preset person', () => {
    expect(emptyDetail('Maria')).toEqual({
      recipient: 'Maria',
      label: '',
      value: '',
    });
    expect(emptyDetail().recipient).toBe('');
  });

  it('asks for every field and limits their length', () => {
    expect(validateDetail(emptyDetail())).toEqual({
      recipient: 'Выберите, для кого',
      label: 'Введите, что это',
      value: 'Введите значение',
    });
    expect(
      validateDetail({
        recipient: 'Maria',
        label: 'x'.repeat(101),
        value: 'y'.repeat(201),
      }),
    ).toEqual({
      recipient: undefined,
      label: 'Не длиннее 100 символов',
      value: 'Не длиннее 200 символов',
    });
  });

  it('trims the request and round-trips a detail', () => {
    expect(
      toDetailRequest({
        recipient: 'Maria',
        label: ' Кольцо ',
        value: ' 18,5–19,0 ',
      }),
    ).toEqual({
      recipient: 'Maria',
      label: 'Кольцо',
      value: '18,5–19,0',
    });
    const detail = {
      id: 'd',
      version: 1,
      recipient: 'Vitaliy' as const,
      label: 'Обувь',
      value: '43',
      sortOrder: 1,
    };
    expect(fromDetail(detail)).toEqual({
      recipient: 'Vitaliy',
      label: 'Обувь',
      value: '43',
    });
  });
});
