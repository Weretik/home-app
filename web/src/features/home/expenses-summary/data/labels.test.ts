import { limitName } from './labels';

test('a quarterly limit says so; a monthly one keeps its name', () => {
  expect(limitName({ name: 'Одежда', period: 'Quarter' })).toBe(
    'Одежда (квартал)',
  );
  expect(limitName({ name: 'Продукты', period: 'Month' })).toBe('Продукты');
});
