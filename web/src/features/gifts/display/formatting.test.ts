import { describe, expect, it } from 'vitest';
import {
  formatLongDay,
  formatOccasion,
  formatPrice,
  formatVariantsCount,
  variantLabel,
} from './formatting';

const nbsp = (text: string) => text.replace(/ /g, ' ');

describe('formatPrice', () => {
  it('says the price is unknown instead of showing zero', () => {
    expect(formatPrice(null)).toBe('цена не указана');
    expect(formatPrice(undefined)).toBe('цена не указана');
  });

  it('shows hryvnias with kopecks', () => {
    expect(nbsp(formatPrice(1234.5))).toBe(nbsp('1 234,50 ₴'));
  });
});

describe('formatLongDay', () => {
  it('reads a contract date as a calendar day, without «г.»', () => {
    expect(formatLongDay('2027-03-12')).toBe('12 марта 2027');
    expect(formatLongDay('2026-01-01')).toBe('1 января 2026');
  });
});

describe('formatOccasion', () => {
  it('joins the occasion and its date', () => {
    expect(formatOccasion('День рождения', '2027-03-12')).toBe(
      'День рождения · 12 марта 2027',
    );
  });

  it('shows whichever part is set', () => {
    expect(formatOccasion(null, '2027-03-12')).toBe('12 марта 2027');
    expect(formatOccasion('Новый год', null)).toBe('Новый год');
    expect(formatOccasion(null, null)).toBeUndefined();
    expect(formatOccasion('  ', undefined)).toBeUndefined();
  });
});

describe('variantLabel', () => {
  it('uses the title when there is one', () => {
    expect(variantLabel({ title: 'Синий', link: 'https://a.ua/x' })).toBe(
      'Синий',
    );
  });

  it('falls back to the shop address without www', () => {
    expect(
      variantLabel({ title: null, link: 'https://www.rozetka.com.ua/x/1' }),
    ).toBe('rozetka.com.ua');
    expect(variantLabel({ title: ' ', link: 'http://prom.ua' })).toBe(
      'prom.ua',
    );
  });

  it('shows the raw link when it is not a URL', () => {
    expect(variantLabel({ link: 'not a url' })).toBe('not a url');
  });
});

describe('formatVariantsCount', () => {
  it('declines the word', () => {
    expect(formatVariantsCount(1)).toBe('1 вариант');
    expect(formatVariantsCount(2)).toBe('2 варианта');
    expect(formatVariantsCount(5)).toBe('5 вариантов');
    expect(formatVariantsCount(21)).toBe('21 вариант');
  });
});
