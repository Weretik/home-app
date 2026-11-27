import { ThemeProvider } from '@mui/material/styles';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { theme } from '../../../app/theme/theme';
import { GiftsSection } from './GiftsSection';

function renderAt(path: string) {
  render(
    <ThemeProvider theme={theme}>
      <MemoryRouter initialEntries={[path]}>
        <Routes>
          <Route path="/gifts" element={<GiftsSection />}>
            <Route index element={<p>список</p>} />
            <Route path="archive" element={<p>архів</p>} />
            <Route path="settings" element={<p>налаштування</p>} />
          </Route>
        </Routes>
      </MemoryRouter>
    </ThemeProvider>,
  );
}

describe('GiftsSection', () => {
  it('shows the title, the tabs and the open page', () => {
    renderAt('/gifts');
    expect(
      screen.getByRole('heading', { level: 1, name: 'Подарунки' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('tab', { name: 'Список' })).toHaveAttribute(
      'aria-selected',
      'true',
    );
    expect(screen.getByText('список')).toBeInTheDocument();
  });

  it('keeps the filters when switching tabs and drops them for the settings', () => {
    renderAt('/gifts?recipient=Maria&category=c1&other=x');
    expect(screen.getByRole('tab', { name: 'Подаровані' })).toHaveAttribute(
      'href',
      '/gifts/archive?recipient=Maria&category=c1',
    );
    expect(screen.getByRole('link', { name: 'Налаштування' })).toHaveAttribute(
      'href',
      '/gifts/settings',
    );
  });

  it('marks the archive tab and the settings gear when they are open', () => {
    renderAt('/gifts/archive');
    expect(screen.getByRole('tab', { name: 'Подаровані' })).toHaveAttribute(
      'aria-selected',
      'true',
    );
    expect(screen.getByText('архів')).toBeInTheDocument();
  });

  it('marks the gear as the current page on the settings', () => {
    renderAt('/gifts/settings');
    expect(screen.getByRole('link', { name: 'Налаштування' })).toHaveAttribute(
      'aria-current',
      'page',
    );
    expect(screen.queryByRole('tab', { selected: true })).toBeNull();
  });
});
