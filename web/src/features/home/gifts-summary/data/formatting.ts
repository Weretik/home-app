import { locale } from '../../../../platform/locale';

const whole = new Intl.NumberFormat(locale, {
  style: 'currency',
  currency: 'UAH',
  currencyDisplay: 'narrowSymbol',
  maximumFractionDigits: 0,
});

const exact = new Intl.NumberFormat(locale, {
  style: 'currency',
  currency: 'UAH',
  currencyDisplay: 'narrowSymbol',
});

const day = new Intl.DateTimeFormat(locale, {
  day: 'numeric',
  month: 'long',
  year: 'numeric',
});

/** A rounded sum for the figure cards: «45 000 ₴». */
export const formatWhole = (value: number) => whole.format(value);

/** A price with kopecks; an unknown price is never shown as zero. */
export const formatPrice = (value: number | null | undefined) =>
  value == null ? 'ціну не вказано' : exact.format(value);

/** A contract date (`2027-03-12`) as «12 березня 2027», read as a calendar day. */
export function formatDay(isoDate: string): string {
  const [year = 0, month = 1, date = 1] = isoDate.split('-').map(Number);
  return day.format(new Date(year, month - 1, date)).replace(/\s?р\.$/, '');
}

/** «1 бажання», «3 бажання», «12 бажань». */
export function formatWishes(count: number): string {
  const forms = { one: 'бажання', few: 'бажання', many: 'бажань' } as const;
  const rule = new Intl.PluralRules(locale).select(count);
  return `${count} ${forms[rule as keyof typeof forms] ?? forms.many}`;
}
