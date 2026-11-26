/** Field messages of a form, by the contract's camelCase field name. */
export type FieldErrors = Record<string, string | undefined>;

/** The API limits of the Gifts module. */
export const MAX_PRICE = 10_000_000;
export const MAX_LINK = 2000;

const priceMessage = 'Цена больше нуля, до двух знаков после запятой';

/**
 * Reads a price the way people type it («1 234,5», «99.99»): null for an empty field (the price is unknown),
 * undefined when it is not a positive number up to 10 000 000 with at most two decimals.
 */
export function parsePrice(text: string): number | null | undefined {
  const plain = text.replace(/\s/g, '').replace(',', '.');
  if (plain === '') return null;
  if (!/^\d+(\.\d{1,2})?$/.test(plain)) return undefined;
  const value = Number(plain);
  return value > 0 && value <= MAX_PRICE ? value : undefined;
}

/** A price as an input shows it: «1234,5», «99»; empty for an unknown one. */
export function priceText(value: number | null | undefined): string {
  if (value == null) return '';
  const [whole, cents = ''] = value.toFixed(2).split('.');
  const trimmed = cents.replace(/0+$/, '');
  return trimmed ? `${whole},${trimmed}` : `${whole}`;
}

/** The message of an optional price field, or undefined when it is fine. */
export function checkPrice(text: string): string | undefined {
  return parsePrice(text) === undefined ? priceMessage : undefined;
}

/** An absolute http/https link up to 2000 characters; empty is fine unless `required` gives its message. */
export function checkLink(
  text: string,
  options: { required?: string } = {},
): string | undefined {
  const link = text.trim();
  if (!link) return options.required;
  if (link.length > MAX_LINK) return `Ссылка не длиннее ${MAX_LINK} символов`;
  try {
    const { protocol } = new URL(link);
    if (protocol === 'http:' || protocol === 'https:') return undefined;
  } catch {
    // Not a URL at all; the same message as a wrong scheme.
  }
  return 'Ссылка должна начинаться с http:// или https://';
}

/** Text that is trimmed first: `required` gives the message of an empty one; then at most `max` characters. */
export function checkLength(
  text: string,
  max: number,
  required?: string,
): string | undefined {
  const value = text.trim();
  if (!value) return required;
  return value.length > max ? `Не длиннее ${max} символов` : undefined;
}

/** An optional date (`YYYY-MM-DD`) must fall in the years 2000–2100. */
export function checkYear(isoDate: string): string | undefined {
  if (!isoDate) return undefined;
  const year = Number(isoDate.slice(0, 4));
  return year >= 2000 && year <= 2100
    ? undefined
    : 'Дата между 2000 и 2100 годом';
}

/** True when no field has a message. */
export const isValid = (errors: FieldErrors) =>
  Object.values(errors).every((message) => !message);
