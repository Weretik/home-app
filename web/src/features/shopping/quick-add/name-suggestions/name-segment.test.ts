import { currentSegment, replaceSegment } from './name-segment';

test('the segment is the text after the last comma, semicolon or new line', () => {
  expect(currentSegment('хле')).toEqual({ start: 0, query: 'хле' });
  expect(currentSegment('молоко, хле')).toEqual({ start: 7, query: 'хле' });
  expect(currentSegment('молоко; яйца 10;сы')).toEqual({
    start: 16,
    query: 'сы',
  });
  expect(currentSegment('молоко\n- хле')).toEqual({ start: 7, query: 'хле' });
});

test('bullets, dashes and spaces around the segment are not part of the query', () => {
  expect(currentSegment('  • зелёный чай  ').query).toBe('зелёный чай');
  expect(currentSegment('молоко, — сыр').query).toBe('сыр');
});

test('an empty segment asks for nothing', () => {
  expect(currentSegment('').query).toBe('');
  expect(currentSegment('молоко, ').query).toBe('');
  expect(currentSegment('молоко,').query).toBe('');
});

test('a chosen name replaces only the segment being typed', () => {
  expect(replaceSegment('хле', 'Хлеб')).toBe('Хлеб');
  expect(replaceSegment('молоко, хле', 'Хлеб')).toBe('молоко, Хлеб');
  expect(replaceSegment('молоко,хле', 'Хлеб')).toBe('молоко, Хлеб');
  expect(replaceSegment('молоко;  - хле', 'Хлеб')).toBe('молоко; Хлеб');
  expect(replaceSegment('молоко\nхле', 'Хлеб')).toBe('молоко\nХлеб');
});
