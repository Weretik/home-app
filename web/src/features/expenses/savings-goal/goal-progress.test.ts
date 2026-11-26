import { goalProgress } from './goal-progress';

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
