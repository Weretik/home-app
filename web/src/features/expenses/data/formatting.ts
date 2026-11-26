const amount = new Intl.NumberFormat('ru-RU', {
  style: 'currency',
  currency: 'UAH',
  currencyDisplay: 'narrowSymbol',
});

const day = new Intl.DateTimeFormat('ru-RU', {
  weekday: 'short',
  day: 'numeric',
  month: 'short',
});

/** An amount in hryvnias: «95 024,00 ₴». */
export function formatAmount(value: number): string {
  return amount.format(value);
}

/**
 * A signed amount with an explicit sign: «+1 500,00 ₴», «−700,00 ₴» (a real minus sign), «0,00 ₴». The sign is
 * always written, so the meaning does not rest on the colour alone.
 */
export function formatSignedAmount(value: number): string {
  if (value > 0) return `+${amount.format(value)}`;
  if (value < 0) return `\u2212${amount.format(-value)}`;
  return amount.format(0);
}

/** A contract date (`2026-10-01`) as a short day: «чт, 1 окт.». Read as a local date, so no time zone shifts it. */
export function formatDay(isoDate: string): string {
  const [year = 0, month = 1, date = 1] = isoDate.split('-').map(Number);
  return day.format(new Date(year, month - 1, date));
}

/** «1 запись», «3 записи», «12 записей». */
export function formatRecordCount(count: number): string {
  const forms = { one: 'запись', few: 'записи', many: 'записей' } as const;
  const rule = new Intl.PluralRules('ru-RU').select(count);
  return `${count} ${forms[rule as keyof typeof forms] ?? forms.many}`;
}
