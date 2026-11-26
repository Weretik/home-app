import { describe, expect, it } from 'vitest';
import type { GiftPriority, Recipient } from '../data/gifts-api';
import { applyFilters, groupByRecipient, parseFilters } from './gift-filters';

const gift = (
  id: string,
  recipient: Recipient,
  categoryId = 'c1',
  priority: GiftPriority = 'Normal',
) => ({
  id,
  recipient,
  categoryId,
  priority,
});

describe('parseFilters', () => {
  it('reads a known recipient and category', () => {
    const params = new URLSearchParams('recipient=Maria&category=c1');
    expect(parseFilters(params, ['c1', 'c2'])).toEqual({
      recipient: 'Maria',
      categoryId: 'c1',
    });
  });

  it('ignores an unknown recipient and a category that does not exist', () => {
    const params = new URLSearchParams('recipient=Bob&category=nope');
    expect(parseFilters(params, ['c1'])).toEqual({});
  });

  it('reads a known priority and ignores an unknown one', () => {
    expect(parseFilters(new URLSearchParams('priority=High'))).toEqual({
      priority: 'High',
    });
    expect(parseFilters(new URLSearchParams('priority=Urgent'))).toEqual({});
  });

  it('keeps any category while the categories are not loaded yet', () => {
    const params = new URLSearchParams('category=c9');
    expect(parseFilters(params)).toEqual({ categoryId: 'c9' });
  });
});

describe('applyFilters', () => {
  const items = [
    gift('1', 'Maria', 'c1'),
    gift('2', 'Vitaliy', 'c2'),
    gift('3', 'Maria', 'c2'),
  ];

  it('keeps the server order of the matching items', () => {
    expect(applyFilters(items, { categoryId: 'c2' }).map((g) => g.id)).toEqual([
      '2',
      '3',
    ]);
    expect(
      applyFilters(items, { recipient: 'Maria', categoryId: 'c2' }).map(
        (g) => g.id,
      ),
    ).toEqual(['3']);
  });

  it('filters by priority together with the other filters', () => {
    const withPriorities = [
      gift('1', 'Maria', 'c1', 'High'),
      gift('2', 'Maria', 'c1', 'Normal'),
      gift('3', 'Vitaliy', 'c1', 'High'),
    ];
    expect(
      applyFilters(withPriorities, { priority: 'High' }).map((g) => g.id),
    ).toEqual(['1', '3']);
    expect(
      applyFilters(withPriorities, {
        priority: 'High',
        recipient: 'Maria',
      }).map((g) => g.id),
    ).toEqual(['1']);
  });

  it('returns everything without filters', () => {
    expect(applyFilters(items, {})).toHaveLength(3);
  });
});

describe('groupByRecipient', () => {
  it('groups in the order Маша, Виталий, Вместе and leaves out empty groups', () => {
    const groups = groupByRecipient([
      gift('1', 'Together'),
      gift('2', 'Maria'),
      gift('3', 'Together'),
    ]);
    expect(groups.map((g) => [g.recipient, g.items.map((i) => i.id)])).toEqual([
      ['Maria', ['2']],
      ['Together', ['1', '3']],
    ]);
  });

  it('sums the known prices of a group', () => {
    const [group] = groupByRecipient([
      { ...gift('1', 'Maria'), price: 1000 },
      { ...gift('2', 'Maria'), price: null },
      { ...gift('3', 'Maria'), price: 625.5 },
    ]);
    expect(group?.priceSum).toBe(1625.5);
  });
});
