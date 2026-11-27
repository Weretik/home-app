import { locale } from '../../../platform/locale';

const money = new Intl.NumberFormat(locale, {
  style: 'currency',
  currency: 'UAH',
  currencyDisplay: 'narrowSymbol',
});

const whole = new Intl.NumberFormat(locale, {
  style: 'currency',
  currency: 'UAH',
  currencyDisplay: 'narrowSymbol',
  maximumFractionDigits: 0,
});

const longDay = new Intl.DateTimeFormat(locale, {
  day: 'numeric',
  month: 'long',
  year: 'numeric',
});

/** A gift price: «1 625 ₴» when whole, «1 234,50 ₴» with real kopecks; an unknown price is never shown as zero. */
export function formatPrice(value: number | null | undefined): string {
  if (value == null) return 'ціну не вказано';
  return Number.isInteger(value) ? whole.format(value) : money.format(value);
}

/** A rounded sum for a person's group header: «4 500 ₴». */
export const formatWhole = (value: number) => whole.format(value);

/** «1 подарунок», «3 подарунки», «12 подарунків». */
export function formatGiftsCount(count: number): string {
  const forms = {
    one: 'подарунок',
    few: 'подарунки',
    many: 'подарунків',
  } as const;
  const rule = new Intl.PluralRules(locale).select(count);
  return `${count} ${forms[rule as keyof typeof forms] ?? forms.many}`;
}

/** A contract date (`2027-03-12`) as «12 березня 2027». Read as a calendar day, so no time zone shifts it. */
export function formatLongDay(isoDate: string): string {
  const [year = 0, month = 1, date = 1] = isoDate.split('-').map(Number);
  return longDay.format(new Date(year, month - 1, date)).replace(/\s?р\.$/, '');
}

/** «День народження · 12 березня 2027», or whichever part is set; undefined when neither is. */
export function formatOccasion(
  occasion: string | null | undefined,
  date: string | null | undefined,
): string | undefined {
  const parts = [occasion?.trim(), date ? formatLongDay(date) : undefined];
  const text = parts.filter(Boolean).join(' · ');
  return text || undefined;
}

/** What a variant link reads as: its title, else the shop's address without `www.`. */
export function variantLabel(variant: {
  title?: string | null;
  link: string;
}): string {
  const title = variant.title?.trim();
  if (title) return title;
  try {
    return new URL(variant.link).hostname.replace(/^www\./, '');
  } catch {
    return variant.link;
  }
}

/** «1 варіант», «2 варіанти», «5 варіантів». */
export function formatVariantsCount(count: number): string {
  const forms = { one: 'варіант', few: 'варіанти', many: 'варіантів' } as const;
  const rule = new Intl.PluralRules(locale).select(count);
  return `${count} ${forms[rule as keyof typeof forms] ?? forms.many}`;
}

/** How an archived gift ended: «Подаровано 26 листопада 2026», «Подаровано», «Неактуально»; undefined while wanted. */
export function formatOutcome(gift: {
  status: string;
  giftedOn?: string | null;
}): string | undefined {
  if (gift.status === 'Gifted')
    return gift.giftedOn
      ? `Подаровано ${formatLongDay(gift.giftedOn)}`
      : 'Подаровано';
  if (gift.status === 'Dropped') return 'Неактуально';
  return undefined;
}
