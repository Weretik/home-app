import { describe, expect, it } from 'vitest';
import type { GiftItem } from '../data/gifts-api';
import {
  MAX_VARIANTS,
  emptyGift,
  fromGift,
  newVariant,
  toGiftRequest,
  validateGift,
  type GiftFormValues,
} from './gift-form';

const filled = (overrides: Partial<GiftFormValues> = {}): GiftFormValues => ({
  ...emptyGift(),
  recipient: 'Maria',
  name: '  Серьги ',
  categoryId: 'c1',
  ...overrides,
});

const item: GiftItem = {
  id: 'g1',
  version: 4,
  recipient: 'Vitaliy',
  name: 'Наушники',
  categoryId: 'c2',
  categoryName: 'Техника',
  priority: 'Someday',
  price: 1234.5,
  link: 'https://a.ua/x',
  status: 'Wanted',
  giftedOn: null,
  occasion: 'Новый год',
  occasionDate: '2026-12-31',
  notes: 'Белые',
  variants: [{ id: 'v1', title: null, link: 'https://b.ua', price: 999 }],
  createdAtUtc: '2026-11-01T00:00:00Z',
};

describe('emptyGift', () => {
  it('starts with «⭐ Хочу» and the filters as preset', () => {
    expect(
      emptyGift({ recipient: 'Together', categoryId: 'c3' }),
    ).toMatchObject({
      recipient: 'Together',
      categoryId: 'c3',
      priority: 'Normal',
      variants: [],
    });
    expect(emptyGift().recipient).toBe('');
  });
});

describe('toGiftRequest', () => {
  it('trims, sends nulls for empty optional fields and keeps the variant order', () => {
    const values = filled({
      variants: [
        {
          ...newVariant(),
          link: ' https://a.ua/1 ',
          title: ' Синий ',
          price: '1 200',
        },
        { ...newVariant(), link: 'https://a.ua/2' },
      ],
    });
    expect(toGiftRequest(values)).toEqual({
      recipient: 'Maria',
      name: 'Серьги',
      categoryId: 'c1',
      priority: 'Normal',
      price: null,
      link: null,
      occasion: null,
      occasionDate: null,
      notes: null,
      variants: [
        { title: 'Синий', link: 'https://a.ua/1', price: 1200 },
        { title: null, link: 'https://a.ua/2', price: null },
      ],
    });
  });

  it('round-trips a gift through the form', () => {
    const request = toGiftRequest(fromGift(item));
    expect(request).toEqual({
      recipient: 'Vitaliy',
      name: 'Наушники',
      categoryId: 'c2',
      priority: 'Someday',
      price: 1234.5,
      link: 'https://a.ua/x',
      occasion: 'Новый год',
      occasionDate: '2026-12-31',
      notes: 'Белые',
      variants: [{ title: null, link: 'https://b.ua', price: 999 }],
    });
    expect(fromGift(item).price).toBe('1234,5');
  });
});

describe('validateGift', () => {
  const messages = (values: GiftFormValues) =>
    Object.fromEntries(
      Object.entries(validateGift(values)).filter(([, m]) => m),
    );

  it('accepts a minimal gift', () => {
    expect(messages(filled())).toEqual({});
  });

  it('names every problem under its field', () => {
    expect(
      messages({
        ...emptyGift(),
        price: '0',
        link: 'shop.ua',
        occasionDate: '1999-01-01',
        variants: [{ ...newVariant(), price: '12,345' }],
      }),
    ).toEqual({
      recipient: 'Выберите, для кого',
      name: 'Введите название',
      categoryId: 'Выберите категорию',
      price: 'Цена больше нуля, до двух знаков после запятой',
      link: 'Ссылка должна начинаться с http:// или https://',
      occasionDate: 'Дата между 2000 и 2100 годом',
      'variants[0].link': 'Укажите ссылку варианта',
      'variants[0].price': 'Цена больше нуля, до двух знаков после запятой',
    });
  });

  it('limits the variants to 20', () => {
    const variants = Array.from({ length: MAX_VARIANTS + 1 }, () => ({
      ...newVariant(),
      link: 'https://a.ua',
    }));
    expect(messages(filled({ variants }))).toEqual({
      variants: 'Не больше 20 вариантов',
    });
  });
});
