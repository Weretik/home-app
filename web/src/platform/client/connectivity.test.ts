import { onlineManager } from '@tanstack/react-query';
import { afterEach, vi } from 'vitest';
import { ApiError, NetworkError } from '../../api-client';
import { markOffline, OFFLINE_PROBE_MS } from './connectivity';
import { retryRead } from './query-client';

afterEach(() => vi.useRealTimers());

test('a request without an answer counts the app offline until the probe, then online again', () => {
  vi.useFakeTimers();
  markOffline();
  expect(onlineManager.isOnline()).toBe(false);

  vi.advanceTimersByTime(OFFLINE_PROBE_MS - 1);
  markOffline();
  vi.advanceTimersByTime(OFFLINE_PROBE_MS - 1);
  expect(onlineManager.isOnline()).toBe(false);

  vi.advanceTimersByTime(1);
  expect(onlineManager.isOnline()).toBe(true);
});

test('a read without an answer waits for the network instead of failing; others retry as before', () => {
  expect(retryRead(10, new NetworkError())).toBe(true);
  expect(onlineManager.isOnline()).toBe(false);

  expect(retryRead(0, new ApiError(503, {}))).toBe(true);
  expect(retryRead(2, new ApiError(503, {}))).toBe(false);
  expect(retryRead(0, new ApiError(404, {}))).toBe(false);
});
