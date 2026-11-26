/** The URL parameter of a link to one gift (web ADR 0013): `/gifts?gift=<id>`. */
export const GIFT_PARAM = 'gift';

export type GiftLinkStep<T> =
  | { kind: 'wait' }
  | { kind: 'open'; item: T }
  | { kind: 'toArchive' }
  | { kind: 'missing' };

/**
 * What a tab does with `?gift=<id>` (gifts 002 FR-005): wait for its list, open the gift it holds, move from «Список»
 * to «Подаренные» when the gift is not wanted any more, or give up when neither tab has it.
 */
export function resolveGiftLink<T extends { id: string }>(
  id: string,
  items: readonly T[] | undefined,
  archived: boolean,
): GiftLinkStep<T> {
  if (!items) return { kind: 'wait' };
  const item = items.find((gift) => gift.id === id);
  if (item) return { kind: 'open', item };
  return archived ? { kind: 'missing' } : { kind: 'toArchive' };
}
