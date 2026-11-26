import { goalPercent, goalProgress } from './goal-progress';

test('gives the whole percent done and what is left', () => {
  expect(goalProgress(42_500, 100_000)).toEqual({
    percent: 42,
    remaining: 57_500,
    done: false,
  });
  expect(goalProgress(99_999.99, 100_000)).toEqual({
    percent: 99,
    remaining: 0.01,
    done: false,
  });
});

test('is done at the goal and past it, and never goes below zero', () => {
  expect(goalProgress(100_000, 100_000)).toEqual({
    percent: 100,
    remaining: 0,
    done: true,
  });
  expect(goalProgress(120_000, 100_000).percent).toBe(120);
  expect(goalProgress(-500, 100_000).percent).toBe(0);
});

// The same cases as money.test.ts of the home summary: one rule for the goal percent on both pages.
test('the goal percent rounds down, so 100 % means the goal is reached', () => {
  expect(goalPercent(42_500, 100_000)).toBe(42);
  expect(goalPercent(99_600, 100_000)).toBe(99);
  expect(goalPercent(100_000, 100_000)).toBe(100);
  expect(goalPercent(120_000, 100_000)).toBe(120);
  expect(goalPercent(-500, 100_000)).toBe(0);
  expect(goalPercent(500, 0)).toBe(0);
});
