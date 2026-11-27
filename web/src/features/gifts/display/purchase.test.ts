import { describe, expect, it } from 'vitest';
import { formatPurchase, kyivToday } from './purchase';

describe('formatPurchase', () => {
  it('is null without a buy-by day', () => {
    expect(
      formatPurchase({ giver: 'Vitaliy', buyBy: null }, '2026-11-27'),
    ).toBeNull();
    expect(formatPurchase({}, '2026-11-27')).toBeNull();
  });

  it('shows the day and the giver', () => {
    expect(
      formatPurchase({ giver: 'Vitaliy', buyBy: '2026-12-12' }, '2026-11-27'),
    ).toEqual({ text: '🛒 купити до 12.12 · дарує Віталій', overdue: false });
    expect(
      formatPurchase({ giver: 'Together', buyBy: '2026-12-12' }, '2026-11-27')
        ?.text,
    ).toBe('🛒 купити до 12.12 · дарує разом');
  });

  it('leaves the giver out when nobody is set', () => {
    expect(formatPurchase({ buyBy: '2027-01-05' }, '2026-11-27')?.text).toBe(
      '🛒 купити до 05.01',
    );
  });

  it('is overdue only after the day', () => {
    expect(formatPurchase({ buyBy: '2026-11-27' }, '2026-11-27')?.overdue).toBe(
      false,
    );
    expect(formatPurchase({ buyBy: '2026-11-26' }, '2026-11-27')?.overdue).toBe(
      true,
    );
  });
});

describe('kyivToday', () => {
  it('is the Kyiv calendar day', () => {
    expect(kyivToday(new Date('2026-11-26T22:30:00Z'))).toBe('2026-11-27');
  });
});
