import { ThemeProvider } from '@mui/material/styles';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import type { GiftItem } from '../data/gifts-api';
import { GiftTable } from './GiftTable';

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
  giver: null,
  buyBy: null,
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

function renderTable(props: Partial<Parameters<typeof GiftTable>[0]> = {}) {
  const handlers = { onStatus: vi.fn(), onEdit: vi.fn(), onDelete: vi.fn() };
  render(
    <ThemeProvider theme={theme}>
      <GiftTable
        items={[gift()]}
        variant="active"
        label="Подарки"
        grouped={false}
        isChanging={() => false}
        {...handlers}
        {...props}
      />
    </ThemeProvider>,
  );
  return handlers;
}

const headers = () =>
  screen.getAllByRole('columnheader').map((cell) => cell.textContent);

describe('GiftTable', () => {
  it('shows a wanted gift in one row with the columns of «Список»', () => {
    renderTable();
    expect(headers()).toEqual([
      'Подарок',
      'Варианты',
      'Категория',
      'Приоритет',
      'Цена',
      'Повод',
      '',
    ]);
    const row = screen.getByRole('row', { name: /Серьги с топазом/ });
    for (const text of [
      'Украшения',
      '🔥 Очень хочу',
      'цена не указана',
      'День рождения · 12 марта 2027',
      'Белое золото',
    ])
      expect(within(row).getByText(text)).toBeInTheDocument();
    const link = within(row).getByRole('link', {
      name: 'Открыть ссылку: Серьги с топазом',
    });
    expect(link).toHaveAttribute('href', 'https://shop.ua/earrings');
    expect(link).toHaveAttribute('target', '_blank');
    expect(link).toHaveAttribute('rel', 'noopener noreferrer');
  });

  it('groups the gifts per person under a header with the count and the sum', () => {
    renderTable({
      grouped: true,
      items: [
        gift({ id: 'a', recipient: 'Vitaliy', name: 'Наушники', price: 1625 }),
        gift({ id: 'b', name: 'Духи', price: 3200 }),
        gift({ id: 'c', name: 'Шарф', price: null }),
      ],
    });
    const titles = screen.getAllByRole('heading', { level: 2 });
    expect(titles.map((title) => title.textContent)).toEqual([
      'Маша',
      'Виталий',
    ]);
    const masha = screen.getByRole('rowheader', { name: /Маша/ });
    expect(masha.textContent?.replace(/\s/g, ' ')).toContain(
      '2 подарка · ≈ 3 200 ₴',
    );
    const vitaliy = screen.getByRole('rowheader', { name: /Виталий/ });
    expect(vitaliy.textContent?.replace(/\s/g, ' ')).toContain(
      '1 подарок · ≈ 1 625 ₴',
    );
    expect(headers()).not.toContain('Для кого');
  });

  it('opens the variants in a row under the gift', async () => {
    renderTable();
    const toggle = screen.getByRole('button', { name: 'Варианты: 2' });
    expect(toggle).toHaveAttribute('aria-expanded', 'false');
    await userEvent.click(toggle);
    expect(toggle).toHaveAttribute('aria-expanded', 'true');
    expect(screen.getByRole('link', { name: /Голубой/ })).toHaveAttribute(
      'href',
      'https://a.ua/1',
    );
    expect(
      screen.getByRole('link', { name: /rozetka\.com\.ua/ }),
    ).toBeInTheDocument();
    await userEvent.click(toggle);
    expect(screen.queryByRole('link', { name: /Голубой/ })).toBeNull();
  });

  it('marks a gift gifted from the row, and not relevant or edited from its menu', async () => {
    const { onStatus, onEdit, onDelete } = renderTable();
    await userEvent.click(
      screen.getByRole('button', { name: 'Подарено: Серьги с топазом' }),
    );
    expect(onStatus).toHaveBeenLastCalledWith(gift(), 'Gifted');
    const menu = () =>
      userEvent.click(
        screen.getByRole('button', { name: 'Действия: Серьги с топазом' }),
      );
    await menu();
    await userEvent.click(
      screen.getByRole('menuitem', { name: 'Не актуально' }),
    );
    expect(onStatus).toHaveBeenLastCalledWith(gift(), 'Dropped');
    await menu();
    await userEvent.click(screen.getByRole('menuitem', { name: 'Изменить' }));
    expect(onEdit).toHaveBeenCalledWith(gift());
    await menu();
    await userEvent.click(screen.getByRole('menuitem', { name: 'Удалить' }));
    expect(onDelete).toHaveBeenCalledWith(gift());
  });

  it('locks the status actions while the change is sent', async () => {
    renderTable({ isChanging: () => true });
    expect(
      screen.getByRole('button', { name: 'Подарено: Серьги с топазом' }),
    ).toBeDisabled();
    await userEvent.click(
      screen.getByRole('button', { name: 'Действия: Серьги с топазом' }),
    );
    expect(
      screen.getByRole('menuitem', { name: 'Не актуально' }),
    ).toHaveAttribute('aria-disabled', 'true');
  });

  it('shows how an archived gift ended and returns it to the list', async () => {
    const gifted = gift({
      status: 'Gifted',
      giftedOn: '2026-11-26',
      price: 1234.5,
    });
    const dropped = gift({
      id: 'g2',
      name: 'Плед',
      status: 'Dropped',
      variants: [],
    });
    const { onStatus } = renderTable({
      items: [gifted, dropped],
      variant: 'archive',
    });
    expect(headers()).toEqual([
      'Подарок',
      'Варианты',
      'Категория',
      'Цена',
      'Итог',
      '',
    ]);
    expect(screen.getByText('Подарено 26 ноября 2026')).toBeInTheDocument();
    expect(screen.getByText('Не актуально')).toBeInTheDocument();
    expect(screen.getByText(/1\s234,50\s₴/)).toBeInTheDocument();
    await userEvent.click(
      screen.getByRole('button', { name: 'Вернуть в список: Плед' }),
    );
    expect(onStatus).toHaveBeenCalledWith(dropped, 'Wanted');
    await userEvent.click(
      screen.getByRole('button', { name: 'Действия: Плед' }),
    );
    expect(screen.queryByRole('menuitem', { name: 'Не актуально' })).toBeNull();
  });
});
