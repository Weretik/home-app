import type { GiftRequest } from '../data/gift-writes-api';
import type { GiftItem, GiftPriority, Recipient } from '../data/gifts-api';
import {
  checkLength,
  checkLink,
  checkPrice,
  checkYear,
  parsePrice,
  priceText,
  type FieldErrors,
} from '../editing/field-checks';

export const MAX_VARIANTS = 20;

/** One variant row as the form holds it; `key` is only for React and focus, never sent. */
export interface VariantValues {
  key: string;
  title: string;
  link: string;
  price: string;
}

/** The gift form: text as typed, turned into a request by {@link toGiftRequest}. */
export interface GiftFormValues {
  recipient: Recipient | '';
  name: string;
  categoryId: string;
  priority: GiftPriority;
  price: string;
  link: string;
  occasion: string;
  occasionDate: string;
  notes: string;
  variants: VariantValues[];
}

/** What a new gift starts with: the list's filters, if any. */
export interface GiftPreset {
  recipient?: Recipient;
  categoryId?: string;
}

let lastKey = 0;
export const newVariant = (): VariantValues => ({
  key: `variant-${++lastKey}`,
  title: '',
  link: '',
  price: '',
});

export const emptyGift = (preset: GiftPreset = {}): GiftFormValues => ({
  recipient: preset.recipient ?? '',
  name: '',
  categoryId: preset.categoryId ?? '',
  priority: 'Normal',
  price: '',
  link: '',
  occasion: '',
  occasionDate: '',
  notes: '',
  variants: [],
});

export const fromGift = (gift: GiftItem): GiftFormValues => ({
  recipient: gift.recipient,
  name: gift.name,
  categoryId: gift.categoryId,
  priority: gift.priority,
  price: priceText(gift.price),
  link: gift.link ?? '',
  occasion: gift.occasion ?? '',
  occasionDate: gift.occasionDate ?? '',
  notes: gift.notes ?? '',
  variants: gift.variants.map((variant) => ({
    ...newVariant(),
    title: variant.title ?? '',
    link: variant.link,
    price: priceText(variant.price),
  })),
});

/** The field messages; server field names are used as keys (`variants[0].link`), so the API's errors land there too. */
export function validateGift(values: GiftFormValues): FieldErrors {
  const errors: FieldErrors = {
    recipient: values.recipient ? undefined : 'Выберите, для кого',
    name: checkLength(values.name, 200, 'Введите название'),
    categoryId: values.categoryId ? undefined : 'Выберите категорию',
    price: checkPrice(values.price),
    link: checkLink(values.link),
    occasion: checkLength(values.occasion, 100),
    occasionDate: checkYear(values.occasionDate),
    notes: checkLength(values.notes, 2000),
    variants:
      values.variants.length > MAX_VARIANTS
        ? `Не больше ${MAX_VARIANTS} вариантов`
        : undefined,
  };
  values.variants.forEach((variant, index) => {
    const at = `variants[${index}]`;
    errors[`${at}.link`] = checkLink(variant.link, {
      required: 'Укажите ссылку варианта',
    });
    errors[`${at}.title`] = checkLength(variant.title, 200);
    errors[`${at}.price`] = checkPrice(variant.price);
  });
  return errors;
}

const orNull = (text: string) => text.trim() || null;

/** The request of valid values: trimmed, empty optional fields as null, variants in the form's order. */
export const toGiftRequest = (values: GiftFormValues): GiftRequest => ({
  recipient: values.recipient as Recipient,
  name: values.name.trim(),
  categoryId: values.categoryId,
  priority: values.priority,
  price: parsePrice(values.price) ?? null,
  link: orNull(values.link),
  occasion: orNull(values.occasion),
  occasionDate: values.occasionDate || null,
  notes: orNull(values.notes),
  variants: values.variants.map((variant) => ({
    title: orNull(variant.title),
    link: variant.link.trim(),
    price: parsePrice(variant.price) ?? null,
  })),
});
