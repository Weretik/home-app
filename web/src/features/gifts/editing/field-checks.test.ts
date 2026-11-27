import { describe, expect, it } from 'vitest';
import {
  checkLength,
  checkLink,
  checkPrice,
  checkYear,
  parsePrice,
  priceText,
} from './field-checks';

describe('parsePrice', () => {
  it('reads prices the way people type them', () => {
    expect(parsePrice('1 234,5')).toBe(1234.5);
    expect(parsePrice('99.99')).toBe(99.99);
    expect(parsePrice('10000000')).toBe(10_000_000);
  });

  it('treats an empty field as an unknown price', () => {
    expect(parsePrice('')).toBeNull();
    expect(parsePrice('  ')).toBeNull();
  });

  it('rejects zero, negatives, three decimals and too much', () => {
    for (const text of ['0', '-5', '12,345', '10000000,01', 'abc'])
      expect(parsePrice(text)).toBeUndefined();
  });
});

describe('checkPrice', () => {
  it('says what a price must be', () => {
    expect(checkPrice('')).toBeUndefined();
    expect(checkPrice('100')).toBeUndefined();
    expect(checkPrice('0')).toBe(
      'Ціна більша за нуль, до двох знаків після коми',
    );
  });
});

describe('checkLink', () => {
  it('accepts empty when optional and http(s) addresses', () => {
    expect(checkLink('')).toBeUndefined();
    expect(checkLink('https://rozetka.com.ua/x')).toBeUndefined();
    expect(checkLink('http://a.ua')).toBeUndefined();
  });

  it('asks for a link when it is required', () => {
    expect(checkLink(' ', { required: 'Вкажіть посилання варіанта' })).toBe(
      'Вкажіть посилання варіанта',
    );
  });

  it('rejects other schemes, bare addresses and too long links', () => {
    const message = 'Посилання має починатися з http:// або https://';
    expect(checkLink('shop.ua/item')).toBe(message);
    expect(checkLink('ftp://a.ua')).toBe(message);
    expect(checkLink(`https://a.ua/${'x'.repeat(2000)}`)).toBe(
      'Посилання не довше 2000 символів',
    );
  });
});

describe('checkLength', () => {
  it('checks required text and its length after trimming', () => {
    expect(checkLength('  ', 200, 'Введіть назву')).toBe('Введіть назву');
    expect(checkLength(' Кольцо ', 200, 'Введіть назву')).toBeUndefined();
    expect(checkLength('x'.repeat(201), 200, 'Введіть назву')).toBe(
      'Не довше 200 символів',
    );
    expect(checkLength('', 100)).toBeUndefined();
  });
});

describe('checkYear', () => {
  it('accepts an empty date and years 2000–2100', () => {
    expect(checkYear('')).toBeUndefined();
    expect(checkYear('2000-01-01')).toBeUndefined();
    expect(checkYear('2100-12-31')).toBeUndefined();
    expect(checkYear('1999-12-31')).toBe('Дата між 2000 і 2100 роком');
    expect(checkYear('2101-01-01')).toBe('Дата між 2000 і 2100 роком');
  });
});

describe('priceText', () => {
  it('shows a price the way it is typed', () => {
    expect(priceText(null)).toBe('');
    expect(priceText(100)).toBe('100');
    expect(priceText(120.5)).toBe('120,5');
    expect(priceText(10.1)).toBe('10,1');
    expect(priceText(99.99)).toBe('99,99');
  });
});
