import { describe, expect, it } from 'vitest';
import {
  addDays,
  formatDue,
  isOverdue,
  kyivDayOf,
  kyivToday,
  weekendOf,
} from './due-date';

const today = '2026-11-26'; // a Thursday

describe('kyivToday', () => {
  it('takes the date in Kyiv, not in UTC', () => {
    // 23:30 UTC on the 26th is already the 27th in Kyiv (UTC+2 in winter).
    expect(kyivToday(new Date('2026-11-26T23:30:00Z'))).toBe('2026-11-27');
    expect(kyivToday(new Date('2026-11-26T12:00:00Z'))).toBe('2026-11-26');
    expect(kyivDayOf('2026-11-26T22:10:00Z')).toBe('2026-11-27');
  });
});

describe('addDays and weekendOf', () => {
  it('crosses months and years', () => {
    expect(addDays('2026-11-30', 1)).toBe('2026-12-01');
    expect(addDays('2026-12-31', 1)).toBe('2027-01-01');
    expect(addDays('2026-03-01', -1)).toBe('2026-02-28');
  });

  it('gives the coming Saturday, or today at the weekend', () => {
    expect(weekendOf(today)).toBe('2026-11-28');
    expect(weekendOf('2026-11-28')).toBe('2026-11-28');
    expect(weekendOf('2026-11-29')).toBe('2026-11-29');
    expect(weekendOf('2026-11-30')).toBe('2026-12-05');
  });
});

describe('isOverdue', () => {
  it('is only a due date before today', () => {
    expect(isOverdue('2026-11-25', today)).toBe(true);
    expect(isOverdue('2026-11-26', today)).toBe(false);
    expect(isOverdue(null, today)).toBe(false);
  });
});

describe('formatDue', () => {
  it('says today, tomorrow and yesterday in words', () => {
    expect(formatDue('2026-11-26', today)).toBe('сьогодні');
    expect(formatDue('2026-11-27', today)).toBe('завтра');
    expect(formatDue('2026-11-25', today)).toBe('вчора');
  });

  it('shows other days with the weekday, and the year outside this one', () => {
    expect(formatDue('2026-11-30', today)).toBe('пн 30.11');
    expect(formatDue('2027-01-04', today)).toBe('пн 04.01.2027');
  });
});
