const whole = new Intl.NumberFormat('ru-RU', {
  style: 'currency',
  currency: 'UAH',
  currencyDisplay: 'narrowSymbol',
  maximumFractionDigits: 0,
});

const exact = new Intl.NumberFormat('ru-RU', {
  style: 'currency',
  currency: 'UAH',
  currencyDisplay: 'narrowSymbol',
});

const day = new Intl.DateTimeFormat('ru-RU', {
  day: 'numeric',
  month: 'long',
  year: 'numeric',
});

/** A rounded sum for the figure cards: «45 000 ₴». */
export const formatWhole = (value: number) => whole.format(value);

/** A price with kopecks; an unknown price is never shown as zero. */
export const formatPrice = (value: number | null | undefined) =>
  value == null ? 'цена не указана' : exact.format(value);

/** A contract date (`2027-03-12`) as «12 марта 2027», read as a calendar day. */
export function formatDay(isoDate: string): string {
  const [year = 0, month = 1, date = 1] = isoDate.split('-').map(Number);
  return day.format(new Date(year, month - 1, date)).replace(/\s?г\.$/, '');
}

/** «1 желание», «3 желания», «12 желаний». */
export function formatWishes(count: number): string {
  const forms = { one: 'желание', few: 'желания', many: 'желаний' } as const;
  const rule = new Intl.PluralRules('ru-RU').select(count);
  return `${count} ${forms[rule as keyof typeof forms] ?? forms.many}`;
}
