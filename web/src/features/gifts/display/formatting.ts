const money = new Intl.NumberFormat('ru-RU', {
  style: 'currency',
  currency: 'UAH',
  currencyDisplay: 'narrowSymbol',
});

const whole = new Intl.NumberFormat('ru-RU', {
  style: 'currency',
  currency: 'UAH',
  currencyDisplay: 'narrowSymbol',
  maximumFractionDigits: 0,
});

const longDay = new Intl.DateTimeFormat('ru-RU', {
  day: 'numeric',
  month: 'long',
  year: 'numeric',
});

/** A gift price: «1 625 ₴» when whole, «1 234,50 ₴» with real kopecks; an unknown price is never shown as zero. */
export function formatPrice(value: number | null | undefined): string {
  if (value == null) return 'цена не указана';
  return Number.isInteger(value) ? whole.format(value) : money.format(value);
}

/** A rounded sum for a person's group header: «4 500 ₴». */
export const formatWhole = (value: number) => whole.format(value);

/** «1 подарок», «3 подарка», «12 подарков». */
export function formatGiftsCount(count: number): string {
  const forms = { one: 'подарок', few: 'подарка', many: 'подарков' } as const;
  const rule = new Intl.PluralRules('ru-RU').select(count);
  return `${count} ${forms[rule as keyof typeof forms] ?? forms.many}`;
}

/** A contract date (`2027-03-12`) as «12 марта 2027». Read as a calendar day, so no time zone shifts it. */
export function formatLongDay(isoDate: string): string {
  const [year = 0, month = 1, date = 1] = isoDate.split('-').map(Number);
  return longDay.format(new Date(year, month - 1, date)).replace(/\s?г\.$/, '');
}

/** «День рождения · 12 марта 2027», or whichever part is set; undefined when neither is. */
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

/** «1 вариант», «2 варианта», «5 вариантов». */
export function formatVariantsCount(count: number): string {
  const forms = { one: 'вариант', few: 'варианта', many: 'вариантов' } as const;
  const rule = new Intl.PluralRules('ru-RU').select(count);
  return `${count} ${forms[rule as keyof typeof forms] ?? forms.many}`;
}

/** How an archived gift ended: «Подарено 26 ноября 2026», «Подарено», «Не актуально»; undefined while wanted. */
export function formatOutcome(gift: {
  status: string;
  giftedOn?: string | null;
}): string | undefined {
  if (gift.status === 'Gifted')
    return gift.giftedOn
      ? `Подарено ${formatLongDay(gift.giftedOn)}`
      : 'Подарено';
  if (gift.status === 'Dropped') return 'Не актуально';
  return undefined;
}
