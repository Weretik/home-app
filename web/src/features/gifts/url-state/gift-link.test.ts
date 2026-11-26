import { describe, expect, it } from 'vitest';
import { resolveGiftLink } from './gift-link';

const gifts = [{ id: 'a' }, { id: 'b' }];

describe('resolveGiftLink', () => {
  it('waits while the list loads', () => {
    expect(resolveGiftLink('a', undefined, false)).toEqual({ kind: 'wait' });
  });

  it('opens the gift the tab holds', () => {
    expect(resolveGiftLink('b', gifts, false)).toEqual({
      kind: 'open',
      item: { id: 'b' },
    });
    expect(resolveGiftLink('a', gifts, true)).toMatchObject({ kind: 'open' });
  });

  it('moves from the list to the archive, then gives up', () => {
    expect(resolveGiftLink('x', gifts, false)).toEqual({ kind: 'toArchive' });
    expect(resolveGiftLink('x', gifts, true)).toEqual({ kind: 'missing' });
  });
});
