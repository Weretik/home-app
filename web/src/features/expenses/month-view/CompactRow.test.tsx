import List from '@mui/material/List';
import { render, screen, within } from '@testing-library/react';
import { CompactRow } from './CompactRow';

test('puts the title, the amount and the actions first, then the details across the row', () => {
  render(
    <List>
      <CompactRow
        divider={false}
        title="Продукти"
        amount="1 250,00 ₴"
        actions={<button type="button">Дії</button>}
        details="27 листопада · Віталій · Картка"
      />
    </List>,
  );

  const row = screen.getByRole('listitem');
  const cells = Array.from(row.children).map((cell) => cell.textContent);
  expect(cells).toEqual([
    'Продукти',
    '1 250,00 ₴',
    'Дії',
    '27 листопада · Віталій · Картка',
  ]);
  expect(within(row).getByText(/27 листопада/)).toHaveStyle({
    gridColumn: '1 / -1',
  });
});

test('leaves the details line out when there is none', () => {
  render(
    <List>
      <CompactRow
        divider={false}
        title="Маша"
        amount="+500,00 ₴"
        actions={null}
      />
    </List>,
  );

  expect(screen.getByRole('listitem').children).toHaveLength(2);
});
