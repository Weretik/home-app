import { itemLine, personName, times } from './summary-labels';

test('names the people in Russian and keeps an unknown value', () => {
  expect(personName('Vitaliy')).toBe('Віталій');
  expect(personName('Maria')).toBe('Маша');
  expect(personName('Guest')).toBe('Guest');
});

test('puts the quantity after the name only when there is one', () => {
  expect(itemLine({ name: 'Молоко', quantity: '2 л' })).toBe('Молоко · 2 л');
  expect(itemLine({ name: 'Хлеб', quantity: null })).toBe('Хлеб');
});

test('counts times with the Ukrainian plural', () => {
  expect([1, 2, 4, 5, 11, 12, 14, 21, 22, 25, 111].map(times)).toEqual([
    '1 раз',
    '2 рази',
    '4 рази',
    '5 разів',
    '11 разів',
    '12 разів',
    '14 разів',
    '21 раз',
    '22 рази',
    '25 разів',
    '111 разів',
  ]);
});
