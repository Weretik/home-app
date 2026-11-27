import { describe, expect, it } from 'vitest';
import {
  formatDay,
  formatPrice,
  formatWhole,
  formatWishes,
} from './formatting';

const plain = (text: string) => text.replace(/\s/g, ' ');

describe('gifts summary formatting', () => {
  it('rounds sums for the figure cards', () => {
    expect(plain(formatWhole(45000.4))).toBe('45 000 ₴');
  });

  it('never shows an unknown price as zero', () => {
    expect(formatPrice(null)).toBe('ціну не вказано');
    expect(plain(formatPrice(1234.5))).toBe('1 234,50 ₴');
  });

  it('shows a contract date as a calendar day', () => {
    expect(formatDay('2027-03-12')).toBe('12 березня 2027');
  });

  it('declines «бажання»', () => {
    expect(formatWishes(1)).toBe('1 бажання');
    expect(formatWishes(3)).toBe('3 бажання');
    expect(formatWishes(12)).toBe('12 бажань');
  });
});
