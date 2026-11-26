import { ThemeProvider } from '@mui/material/styles';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import type { GiftItem } from '../data/gifts-api';
import { GiftCard } from './GiftCard';

const gift = (overrides: Partial<GiftItem> = {}): GiftItem => ({
  id: 'g1',
  version: 3,
  recipient: 'Maria',
  name: 'Серьги с топазом',
  categoryId: 'c-jewel',
  categoryName: 'Украшения',
  priority: 'High',
  price: null,
  link: 'https://shop.ua/earrings',
  status: 'Wanted',
  giftedOn: null,
  occasion: 'День рождения',
  occasionDate: '2027-03-12',
  notes: 'Белое золото',
  variants: [
    { id: 'v1', title: 'Голубой', link: 'https://a.ua/1', price: 4500 },
    {
      id: 'v2',
      title: null,
      link: 'https://www.rozetka.com.ua/2',
      price: null,
    },
  ],
  createdAtUtc: '2026-11-01T10:00:00Z',
  ...overrides,
});

function renderCard(props: Partial<Parameters<typeof GiftCard>[0]> = {}) {
  const handlers = { onStatus: vi.fn(), onEdit: vi.fn(), onDelete: vi.fn() };
  render(
    <ThemeProvider theme={theme}>
      <GiftCard gift={gift()} variant="active" {...handlers} {...props} />
    </ThemeProvider>,
  );
  return handlers;
}

describe('GiftCard', () => {
  it('shows what the wish is', () => {
    renderCard();
    const card = screen.getByRole('article', { name: 'Серьги с топазом' });
    for (const text of [
      'Украшения',
      '🔥 Очень хочу',
      'цена не указана',
      'День рождения · 12 марта 2027',
      'Белое золото',
    ])
      expect(within(card).getByText(text)).toBeInTheDocument();
    const link = within(card).getByRole('link', { name: 'Ссылка' });
    expect(link).toHaveAttribute('href', 'https://shop.ua/earrings');
    expect(link).toHaveAttribute('target', '_blank');
    expect(link).toHaveAttribute('rel', 'noopener noreferrer');
  });

  it('opens and folds the variants', async () => {
    renderCard();
    const toggle = screen.getByRole('button', { name: '2 варианта' });
    expect(toggle).toHaveAttribute('aria-expanded', 'false');
    await userEvent.click(toggle);
    expect(toggle).toHaveAttribute('aria-expanded', 'true');
    expect(screen.getByRole('link', { name: /Голубой/ })).toHaveAttribute(
      'href',
      'https://a.ua/1',
    );
    expect(
      screen.getByRole('link', { name: /rozetka\.com\.ua/ }),
    ).toHaveAttribute('target', '_blank');
  });

  it('marks a wanted wish gifted, or not relevant from the menu', async () => {
    const { onStatus, onEdit } = renderCard();
    await userEvent.click(screen.getByRole('button', { name: 'Подарено' }));
    expect(onStatus).toHaveBeenLastCalledWith(gift(), 'Gifted');
    await userEvent.click(
      screen.getByRole('button', { name: 'Действия: Серьги с топазом' }),
    );
    await userEvent.click(
      screen.getByRole('menuitem', { name: 'Не актуально' }),
    );
    expect(onStatus).toHaveBeenLastCalledWith(gift(), 'Dropped');
    await userEvent.click(
      screen.getByRole('button', { name: 'Действия: Серьги с топазом' }),
    );
    await userEvent.click(screen.getByRole('menuitem', { name: 'Изменить' }));
    expect(onEdit).toHaveBeenCalledWith(gift());
  });

  it('disables the status actions while its change is sent', async () => {
    renderCard({ isPending: true });
    expect(screen.getByRole('button', { name: 'Подарено' })).toBeDisabled();
    await userEvent.click(
      screen.getByRole('button', { name: 'Действия: Серьги с топазом' }),
    );
    expect(
      screen.getByRole('menuitem', { name: 'Не актуально' }),
    ).toHaveAttribute('aria-disabled', 'true');
  });

  it('shows the gifted day and returns a wish to the list in the archive', async () => {
    const gifted = gift({ status: 'Gifted', giftedOn: '2026-11-26' });
    const { onStatus } = renderCard({ gift: gifted, variant: 'archive' });
    expect(screen.getByText('Подарено 26 ноября 2026')).toBeInTheDocument();
    await userEvent.click(
      screen.getByRole('button', { name: 'Вернуть в список' }),
    );
    expect(onStatus).toHaveBeenCalledWith(gifted, 'Wanted');
    await userEvent.click(
      screen.getByRole('button', { name: 'Действия: Серьги с топазом' }),
    );
    expect(screen.queryByRole('menuitem', { name: 'Не актуально' })).toBeNull();
  });

  it('marks a dropped wish and can name the recipient', () => {
    renderCard({
      gift: gift({ status: 'Dropped', price: 1234.5, variants: [] }),
      variant: 'archive',
      showRecipient: true,
    });
    expect(screen.getByText('Не актуально')).toBeInTheDocument();
    expect(screen.getByText('Маша')).toBeInTheDocument();
    expect(screen.getByText(/1\s234,50\s₴/)).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /вариант/ })).toBeNull();
  });
});
