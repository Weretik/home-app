import { ThemeProvider } from '@mui/material/styles';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';
import { theme } from '../../../app/theme/theme';
import type { GiftItem } from '../data/gifts-api';
import { GiftList } from './GiftList';

const gift = (overrides: Partial<GiftItem> = {}): GiftItem => ({
  id: 'g1',
  version: 3,
  recipient: 'Vitaliy',
  name: 'Наушники',
  categoryId: 'c-tech',
  categoryName: 'Техника',
  priority: 'High',
  price: 4500,
  link: null,
  status: 'Wanted',
  giftedOn: null,
  occasion: null,
  occasionDate: '2027-03-12',
  giver: null,
  buyBy: null,
  notes: null,
  variants: [{ id: 'v1', title: 'Белые', link: 'https://a.ua/1', price: null }],
  createdAtUtc: '2026-11-01T10:00:00Z',
  ...overrides,
});

function renderList(props: Partial<Parameters<typeof GiftList>[0]> = {}) {
  const handlers = { onStatus: vi.fn(), onEdit: vi.fn(), onDelete: vi.fn() };
  render(
    <ThemeProvider theme={theme}>
      <GiftList
        items={[gift()]}
        variant="active"
        label="Подарунки"
        grouped={false}
        isChanging={() => false}
        {...handlers}
        {...props}
      />
    </ThemeProvider>,
  );
  return handlers;
}

describe('GiftList', () => {
  it('shows a compact row: name and price, then the category and small chips', () => {
    renderList();
    const row = within(
      screen.getByRole('list', { name: 'Подарунки' }),
    ).getAllByRole('listitem')[0]!;
    expect(within(row).getByText('Наушники')).toBeInTheDocument();
    expect(within(row).getByText(/^4\s500\s₴$/)).toBeInTheDocument();
    expect(within(row).getByText('Техника')).toBeInTheDocument();
    expect(within(row).getByText('🔥 Дуже хочу')).toBeInTheDocument();
    expect(within(row).getByText('12 березня 2027')).toBeInTheDocument();
  });

  it('puts each person in a block with their header, the count and the sum', () => {
    renderList({
      grouped: true,
      items: [
        gift(),
        gift({ id: 'g2', recipient: 'Maria', name: 'Духи', price: 3200 }),
        gift({ id: 'g3', recipient: 'Together', name: 'Плед', price: null }),
      ],
    });
    const masha = screen.getByRole('region', { name: 'Маша' });
    expect(within(masha).getByText('Духи')).toBeInTheDocument();
    expect(masha.textContent?.replace(/\s/g, ' ')).toContain(
      '1 подарунок · ≈ 3 200 ₴',
    );
    const together = screen.getByRole('region', { name: 'Разом' });
    expect(within(together).getByText('1 подарунок')).toBeInTheDocument();
    expect(
      screen
        .getAllByRole('heading', { level: 2 })
        .map((title) => title.textContent),
    ).toEqual(['Маша', 'Віталій', 'Разом']);
    expect(
      within(screen.getByRole('region', { name: 'Віталій' })).getByRole(
        'list',
        { name: 'Подарунки: Віталій' },
      ),
    ).toBeInTheDocument();
  });

  it('opens the variants under the row', async () => {
    renderList();
    await userEvent.click(screen.getByRole('button', { name: '1 варіант' }));
    expect(screen.getByRole('link', { name: /Белые/ })).toHaveAttribute(
      'href',
      'https://a.ua/1',
    );
  });

  it('offers every action in the row menu', async () => {
    const { onStatus } = renderList();
    await userEvent.click(
      screen.getByRole('button', { name: 'Дії: Наушники' }),
    );
    const items = screen
      .getAllByRole('menuitem')
      .map((item) => item.textContent);
    expect(items).toEqual(['Подаровано', 'Неактуально', 'Змінити', 'Видалити']);
    await userEvent.click(screen.getByRole('menuitem', { name: 'Подаровано' }));
    expect(onStatus).toHaveBeenCalledWith(gift(), 'Gifted');
  });

  it('shows how an archived gift ended and offers to return it', async () => {
    const { onStatus } = renderList({
      items: [gift({ status: 'Gifted', giftedOn: '2026-11-26' })],
      variant: 'archive',
    });
    expect(
      screen.getByText('Подаровано 26 листопада 2026'),
    ).toBeInTheDocument();
    expect(screen.getByText('Техника')).toBeInTheDocument();
    await userEvent.click(
      screen.getByRole('button', { name: 'Дії: Наушники' }),
    );
    await userEvent.click(
      screen.getByRole('menuitem', { name: 'Повернути до списку' }),
    );
    expect(onStatus).toHaveBeenCalledWith(
      gift({ status: 'Gifted', giftedOn: '2026-11-26' }),
      'Wanted',
    );
  });
});
