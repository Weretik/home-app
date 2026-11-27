import { ThemeProvider } from '@mui/material/styles';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ColorModeMenu } from './ColorModeMenu';
import { theme } from './theme';

function renderMenu() {
  render(
    <ThemeProvider theme={theme} defaultMode="system">
      <ColorModeMenu />
    </ThemeProvider>,
  );
}

beforeEach(() => localStorage.clear());

test('starts with the system theme', async () => {
  renderMenu();

  await userEvent.click(
    screen.getByRole('button', { name: 'Тема оформлення' }),
  );

  expect(screen.getByRole('menuitem', { name: 'Як у системі' })).toHaveClass(
    'Mui-selected',
  );
});

test('switches to the dark theme and remembers it', async () => {
  renderMenu();

  await userEvent.click(
    screen.getByRole('button', { name: 'Тема оформлення' }),
  );
  await userEvent.click(screen.getByRole('menuitem', { name: 'Темна' }));

  expect(document.documentElement).toHaveAttribute('data-color-scheme', 'dark');
  expect(localStorage.getItem('mui-mode')).toBe('dark');
});
