import type { Giver } from '../data/gifts-api';
import { recipientLabels } from './labels';

const kyivDay = new Intl.DateTimeFormat('en-CA', {
  timeZone: 'Europe/Kyiv',
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
});

/** Today in Europe/Kyiv as «YYYY-MM-DD» (the Todos feature keeps its own copy: features never import each other). */
export function kyivToday(now: Date = new Date()): string {
  return kyivDay.format(now);
}

/** How the giver reads after «дарит»: «Маша», «Виталий», «вместе». */
const giverAfterVerb: Record<Giver, string> = {
  Maria: recipientLabels.Maria,
  Vitaliy: recipientLabels.Vitaliy,
  Together: 'вместе',
};

export interface PurchaseLine {
  text: string;
  /** The buy-by day has passed. */
  overdue: boolean;
}

/**
 * The line under a wanted gift's name (gifts 002 FR-004): «🛒 купить до 12.12 · дарит Виталий», or null without a
 * buy-by day. Contract days compare as strings.
 */
export function formatPurchase(
  gift: { buyBy?: string | null; giver?: Giver | null },
  today: string,
): PurchaseLine | null {
  if (!gift.buyBy) return null;
  const [, month = '', day = ''] = gift.buyBy.split('-');
  const giver = gift.giver ? ` · дарит ${giverAfterVerb[gift.giver]}` : '';
  return {
    text: `🛒 купить до ${day}.${month}${giver}`,
    overdue: gift.buyBy < today,
  };
}
