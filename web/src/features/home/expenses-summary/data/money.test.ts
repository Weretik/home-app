import { formatExact, formatWhole, goalPercentOf, percentOf } from './money';

const normalized = (text: string) => text.replace(/\s/g, ' ');

test('formats rounded and exact amounts with a real minus sign', () => {
  expect(normalized(formatWhole(95_024.4))).toBe('95 024 ₴');
  expect(normalized(formatWhole(-1_200))).toBe('−1 200 ₴');
  expect(normalized(formatExact(-1_200.5))).toBe('−1 200,50 ₴');
});

test('a share rounds to the nearest whole percent, 0 without a total', () => {
  expect(percentOf(1, 3)).toBe(33);
  expect(percentOf(2, 3)).toBe(67);
  expect(percentOf(5, 0)).toBe(0);
});

// The same cases as goal-progress.test.ts on «Скарбничка»: one rule for the goal percent on both pages.
test('the goal percent rounds down, so 100 % means the goal is reached', () => {
  expect(goalPercentOf(42_500, 100_000)).toBe(42);
  expect(goalPercentOf(99_600, 100_000)).toBe(99);
  expect(goalPercentOf(100_000, 100_000)).toBe(100);
  expect(goalPercentOf(120_000, 100_000)).toBe(120);
  expect(goalPercentOf(-500, 100_000)).toBe(0);
  expect(goalPercentOf(500, 0)).toBe(0);
});
