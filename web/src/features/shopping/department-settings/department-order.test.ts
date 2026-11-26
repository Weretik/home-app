import { moveId } from './department-order';

test('moves a department one place up or down', () => {
  expect(moveId(['a', 'b', 'c'], 'b', -1)).toEqual(['b', 'a', 'c']);
  expect(moveId(['a', 'b', 'c'], 'b', 1)).toEqual(['a', 'c', 'b']);
});

test('keeps the order at the ends and for an unknown id', () => {
  const ids = ['a', 'b'];
  expect(moveId(ids, 'a', -1)).toBe(ids);
  expect(moveId(ids, 'b', 1)).toBe(ids);
  expect(moveId(ids, 'x', 1)).toBe(ids);
});
