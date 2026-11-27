import { formatWhen, itemTitle, personLabel, personLabels } from './labels';

test('names the people in Russian and keeps an unknown value', () => {
  expect(personLabels).toEqual({ Vitaliy: 'Віталій', Maria: 'Маша' });
  expect(personLabel('Maria')).toBe('Маша');
  expect(personLabel('Guest')).toBe('Guest');
  expect(personLabel(null)).toBe('');
});

test('shows only the time today and the day before it otherwise', () => {
  const now = new Date(2026, 10, 26, 18, 0);
  const today = new Date(2026, 10, 26, 14, 5).toISOString();
  const earlier = new Date(2026, 10, 25, 14, 5).toISOString();

  expect(formatWhen(today, now)).toBe('14:05');
  expect(formatWhen(earlier, now)).toMatch(/^25 лист\.?, 14:05$/);
});

test('puts the quantity after the name only when there is one', () => {
  expect(itemTitle({ name: 'Молоко', quantity: '2 л' })).toBe('Молоко · 2 л');
  expect(itemTitle({ name: 'Хлеб', quantity: null })).toBe('Хлеб');
});
