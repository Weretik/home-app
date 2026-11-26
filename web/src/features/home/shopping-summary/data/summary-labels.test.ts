import { itemLine, personName, times } from './summary-labels';

test('names the people in Russian and keeps an unknown value', () => {
  expect(personName('Vitaliy')).toBe('Виталий');
  expect(personName('Maria')).toBe('Маша');
  expect(personName('Guest')).toBe('Guest');
});

test('puts the quantity after the name only when there is one', () => {
  expect(itemLine({ name: 'Молоко', quantity: '2 л' })).toBe('Молоко · 2 л');
  expect(itemLine({ name: 'Хлеб', quantity: null })).toBe('Хлеб');
});

test('counts times with the Russian plural', () => {
  expect([1, 2, 4, 5, 11, 12, 14, 21, 22, 25, 111].map(times)).toEqual([
    '1 раз',
    '2 раза',
    '4 раза',
    '5 раз',
    '11 раз',
    '12 раз',
    '14 раз',
    '21 раз',
    '22 раза',
    '25 раз',
    '111 раз',
  ]);
});
