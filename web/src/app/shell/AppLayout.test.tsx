import { ThemeProvider } from '@mui/material/styles';
import { QueryClientProvider } from '@tanstack/react-query';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { afterEach, vi } from 'vitest';
import { createQueryClient } from '../../platform/query-client';
import { theme } from '../theme/theme';
import { AppLayout } from './AppLayout';

const mainSections = ['Головна', 'Бюджет', 'Покупки', 'Справи', 'Подарунки'];

// The header's member menu reads GET /api/identity/me.
const member = { email: 'v@example.com', name: 'Віталій', pictureUrl: null };

function renderAt(path: string) {
  vi.stubGlobal(
    'fetch',
    vi.fn(() =>
      Promise.resolve(
        new Response(JSON.stringify(member), {
          headers: { 'Content-Type': 'application/json' },
        }),
      ),
    ),
  );
  render(
    <ThemeProvider theme={theme}>
      <QueryClientProvider client={createQueryClient()}>
        <MemoryRouter initialEntries={[path]}>
          <Routes>
            <Route element={<AppLayout />}>
              <Route path="*" element={<p>Сторінка</p>} />
            </Route>
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>
    </ThemeProvider>,
  );
}

const sideMenu = () => screen.getByRole('navigation', { name: 'Меню' });

/**
 * jsdom ignores media queries, so the phone's bottom bar ends up display:none here
 * and Testing Library gives it no accessible name; find it by its label instead.
 */
function bottomBarLinks() {
  const bar = document.querySelector('nav[aria-label="Розділи"]');
  if (!(bar instanceof HTMLElement)) throw new Error('No bottom bar');
  return within(bar).getAllByRole('link', { hidden: true });
}

beforeEach(() => localStorage.clear());
afterEach(() => vi.unstubAllGlobals());

test('the side menu lists the main sections, «Про нас» and the service pages', () => {
  renderAt('/');

  for (const name of [...mainSections, 'Про нас', 'Стан'])
    expect(within(sideMenu()).getByRole('link', { name })).toBeInTheDocument();
});

test('the bottom bar lists the five main sections', () => {
  renderAt('/');

  expect(bottomBarLinks().map((link) => link.textContent)).toEqual(
    mainSections,
  );
});

test('«Про нас» is in the side menu only, the bottom bar keeps five', () => {
  renderAt('/notes');

  expect(
    within(sideMenu()).getByRole('link', { name: 'Про нас' }),
  ).toHaveAttribute('aria-current', 'page');
  expect(bottomBarLinks()).toHaveLength(5);
});

test('marks the current section in both menus', () => {
  renderAt('/expenses');

  expect(
    within(sideMenu()).getByRole('link', { name: 'Бюджет' }),
  ).toHaveAttribute('aria-current', 'page');
  expect(
    within(sideMenu()).getByRole('link', { name: 'Головна' }),
  ).not.toHaveAttribute('aria-current');
  const selected = bottomBarLinks().filter((link) =>
    link.classList.contains('Mui-selected'),
  );
  expect(selected.map((link) => link.textContent)).toEqual(['Бюджет']);
});

test('collapses the side menu to icons and remembers it', async () => {
  renderAt('/');

  await userEvent.click(screen.getByRole('button', { name: 'Згорнути меню' }));

  expect(within(sideMenu()).queryByText('Бюджет')).not.toBeInTheDocument();
  expect(
    within(sideMenu()).getByRole('link', { name: 'Бюджет' }),
  ).toBeInTheDocument();
  expect(localStorage.getItem('home.side-nav.collapsed')).toBe('true');
  expect(
    screen.getByRole('button', { name: 'Розгорнути меню' }),
  ).toBeInTheDocument();
});
