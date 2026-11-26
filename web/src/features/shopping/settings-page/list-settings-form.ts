import type { ShoppingSettings, ShoppingSettingsRequest } from './settings-api';

export interface ListSettingsValues {
  cartKeepHours: string;
  frequentDays: string;
}

export const HOURS = { min: 1, max: 72 } as const;
export const DAYS = { min: 30, max: 730 } as const;

export const listSettingsValues = (
  settings: ShoppingSettings,
): ListSettingsValues => ({
  cartKeepHours: String(settings.cartKeepHours),
  frequentDays: String(settings.frequentDays),
});

const whole = (text: string) =>
  /^\d+$/.test(text.trim()) ? Number(text.trim()) : NaN;

function checkRange(
  text: string,
  range: { min: number; max: number },
): string | undefined {
  const value = whole(text);
  if (Number.isNaN(value)) return 'Введите целое число';
  if (value < range.min || value > range.max)
    return `От ${range.min} до ${range.max}`;
  return undefined;
}

/** The messages under the fields; empty when the form can be sent. */
export function checkListSettings(values: ListSettingsValues) {
  const errors: Partial<Record<keyof ListSettingsValues, string>> = {};
  const hours = checkRange(values.cartKeepHours, HOURS);
  const days = checkRange(values.frequentDays, DAYS);
  if (hours) errors.cartKeepHours = hours;
  if (days) errors.frequentDays = days;
  return errors;
}

export const listSettingsRequest = (
  values: ListSettingsValues,
): ShoppingSettingsRequest => ({
  cartKeepHours: whole(values.cartKeepHours),
  frequentDays: whole(values.frequentDays),
});
