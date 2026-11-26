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

/** A rounded amount for the summary cards: «95 024 ₴», «−1 200 ₴» (a real minus sign). */
export function formatWhole(value: number): string {
  const text = whole.format(Math.abs(value));
  return Math.round(value) < 0 ? `−${text}` : text;
}

/** An amount with kopecks for tables: «95 024,00 ₴», «−1 200,50 ₴». */
export function formatExact(value: number): string {
  const text = exact.format(Math.abs(value));
  return value < 0 ? `−${text}` : text;
}

/** A share in whole percent, 0 when there is nothing to compare with. */
export const percentOf = (part: number, total: number) =>
  total > 0 ? Math.round((part / total) * 100) : 0;
