import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { vi } from 'vitest';
import { UpdatePrompt } from './UpdatePrompt';

const register = vi.hoisted(() => ({
  needRefresh: false,
  setNeedRefresh: vi.fn(),
  updateServiceWorker: vi.fn(() => Promise.resolve()),
}));

vi.mock('virtual:pwa-register/react', () => ({
  useRegisterSW: () => ({
    needRefresh: [register.needRefresh, register.setNeedRefresh],
    offlineReady: [false, vi.fn()],
    updateServiceWorker: register.updateServiceWorker,
  }),
}));

test('says nothing while no new version waits', () => {
  register.needRefresh = false;
  render(<UpdatePrompt />);

  expect(screen.queryByText('Доступна нова версія')).not.toBeInTheDocument();
});

test('offers a waiting version and activates it with a reload on «Оновити»', async () => {
  register.needRefresh = true;
  render(<UpdatePrompt />);

  expect(screen.getByText('Доступна нова версія')).toBeInTheDocument();
  await userEvent.click(screen.getByRole('button', { name: 'Оновити' }));

  expect(register.updateServiceWorker).toHaveBeenCalledWith(true);
});
