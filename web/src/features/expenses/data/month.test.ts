import { currentMonth, formatMonth, parseMonth, shiftMonth } from './month';

test('takes the current month from the date', () => {
  expect(currentMonth(new Date(2026, 10, 26))).toEqual({
    year: 2026,
    month: 11,
  });
});

test('shifts across year boundaries', () => {
  expect(shiftMonth({ year: 2026, month: 1 }, -1)).toEqual({
    year: 2025,
    month: 12,
  });
  expect(shiftMonth({ year: 2026, month: 12 }, 1)).toEqual({
    year: 2027,
    month: 1,
  });
  expect(shiftMonth({ year: 2026, month: 10 }, 0)).toEqual({
    year: 2026,
    month: 10,
  });
});

test('reads a month from URL values and falls back on bad ones', () => {
  const fallback = { year: 2026, month: 11 };
  expect(parseMonth('2026', '10', fallback)).toEqual({
    year: 2026,
    month: 10,
  });
  expect(parseMonth(null, null, fallback)).toBe(fallback);
  expect(parseMonth('2026', '13', fallback)).toBe(fallback);
  expect(parseMonth('abc', '5', fallback)).toBe(fallback);
  expect(parseMonth('2026', '1.5', fallback)).toBe(fallback);
});

test('names the month in Russian with the year', () => {
  expect(formatMonth({ year: 2026, month: 10 })).toBe('Октябрь 2026');
  expect(formatMonth({ year: 2027, month: 1 })).toBe('Январь 2027');
});
