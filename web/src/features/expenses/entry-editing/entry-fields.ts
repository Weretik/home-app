import { isoDay, monthDays, type Month } from '../data/month';

/** Field messages of a form, by the contract's camelCase field name. */
export type FieldErrors = Record<string, string | undefined>;

/** The API limits (EntryRules on the backend). */
export const MAX_AMOUNT = 10_000_000;
export const MAX_COMMENT = 500;

/** Today as the API writes a day: «2026-11-26». */
export const todayIso = (today: Date = new Date()) =>
  isoDay(today.getFullYear(), today.getMonth() + 1, today.getDate());

/** The date a new entry starts with: today when it is within the days shown, else their first day. */
export function defaultDateIn(
  days: { from: string; to: string },
  today: Date = new Date(),
) {
  const day = todayIso(today);
  return day >= days.from && day <= days.to ? day : days.from;
}

/** The date a new entry starts with: today in the current month, else the 1st of the open month. */
export function defaultEntryDate(month: Month, today: Date = new Date()) {
  return defaultDateIn(monthDays(month), today);
}

/**
 * Reads an amount the way people type it: «1 234,5», «1234.50». Undefined unless it is a positive number with at
 * most two decimals.
 */
export function parseAmount(text: string): number | undefined {
  const plain = text.replace(/\s/g, '').replace(',', '.');
  if (!/^\d+(\.\d{1,2})?$/.test(plain)) return undefined;
  return Number(plain);
}

/** An amount as an input shows it: «1234,5» stays short, «1234.5» becomes «1234,50». */
export function amountText(value: number): string {
  return Math.abs(value).toFixed(2).replace(/\.00$/, '').replace('.', ',');
}

export function amountError(text: string): string | undefined {
  if (text.trim() === '') return 'Введіть суму';
  const value = parseAmount(text);
  if (value === undefined)
    return 'Сума — число, не більше двох знаків після коми';
  if (value <= 0) return 'Сума має бути більшою за нуль';
  if (value > MAX_AMOUNT) return 'Завелика сума';
  return undefined;
}

export function dateError(text: string): string | undefined {
  if (!text) return 'Виберіть дату';
  const year = Number(text.slice(0, 4));
  if (!/^\d{4}-\d{2}-\d{2}$/.test(text) || year < 2000 || year > 2100)
    return 'Дата між 2000 і 2100 роком';
  return undefined;
}

export function commentError(text: string): string | undefined {
  return text.length > MAX_COMMENT
    ? `Не довше ${MAX_COMMENT} символів`
    : undefined;
}

/** A trimmed comment, or null when it is empty. */
export const commentValue = (text: string) => text.trim() || null;

/** The props that show a field's message under a TextField. */
export function fieldMessage(errors: FieldErrors, name: string) {
  const message = errors[name];
  return { error: Boolean(message), helperText: message };
}

/** True when no field has a message. */
export const isValid = (errors: FieldErrors) =>
  Object.values(errors).every((message) => !message);
