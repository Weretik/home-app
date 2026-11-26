/** Shared look of the limit amounts: the table and the phone list. */
export const limitsLabel = 'Лимиты месяца';
export const number = {
  whiteSpace: 'nowrap',
  fontVariantNumeric: 'tabular-nums',
};
export const leftColor = (left: number | null) =>
  left !== null && left < 0 ? 'error.main' : 'inherit';
export const orDash = (value: number | null, format: (v: number) => string) =>
  value === null ? '—' : format(value);
