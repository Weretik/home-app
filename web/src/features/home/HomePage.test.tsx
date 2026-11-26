import { ThemeProvider } from '@mui/material/styles';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { theme } from '../../app/theme/theme';
import { HomePage } from './HomePage';

test('links every dashboard card to its section', () => {
  render(
    <ThemeProvider theme={theme}>
      <MemoryRouter>
        <HomePage />
      </MemoryRouter>
    </ThemeProvider>,
  );

  const links = {
    'Расходы за месяц': '/expenses',
    'Дела на сегодня': '/todos',
    Покупки: '/shopping',
    Подарки: '/gifts',
    Состояние: '/status',
  };
  for (const [name, href] of Object.entries(links))
    expect(
      screen.getByRole('link', { name: new RegExp(name) }),
    ).toHaveAttribute('href', href);
});
