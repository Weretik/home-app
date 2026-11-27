import { onlineManager } from '@tanstack/react-query';
import { afterEach, vi } from 'vitest';
import { startOfflineSync } from './offline-sync';

afterEach(() => vi.useRealTimers());

test('loads at once, when the network is back, when the app is shown and on an interval', () => {
  vi.useFakeTimers();
  const sync = vi.fn();
  const stop = startOfflineSync(sync, 1000);
  expect(sync).toHaveBeenCalledTimes(1);

  onlineManager.setOnline(false);
  vi.advanceTimersByTime(1000);
  expect(sync).toHaveBeenCalledTimes(1);

  onlineManager.setOnline(true);
  expect(sync).toHaveBeenCalledTimes(2);

  document.dispatchEvent(new Event('visibilitychange'));
  expect(sync).toHaveBeenCalledTimes(3);

  vi.advanceTimersByTime(1000);
  expect(sync).toHaveBeenCalledTimes(4);

  stop();
  vi.advanceTimersByTime(5000);
  onlineManager.setOnline(false);
  onlineManager.setOnline(true);
  expect(sync).toHaveBeenCalledTimes(4);
});
