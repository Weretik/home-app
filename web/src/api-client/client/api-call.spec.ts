import { apiCall } from './api-call';
import { ApiError } from './api-error';
import { NetworkError, NETWORK_ERROR_MESSAGE } from './network-error';

const answer = (status: number, body?: unknown) => ({
  data: status < 400 ? body : undefined,
  error: status >= 400 ? body : undefined,
  response: new Response(null, { status }),
});

test('returns the data of a success and throws an ApiError for an error answer', async () => {
  await expect(
    apiCall(Promise.resolve(answer(200, { id: 1 }))),
  ).resolves.toEqual({ id: 1 });
  await expect(
    apiCall(Promise.resolve(answer(404, { code: 'gone' }))),
  ).rejects.toBeInstanceOf(ApiError);
});

test('a request without an answer throws a NetworkError with a message to show', async () => {
  const failure = apiCall(Promise.reject(new TypeError('Failed to fetch')));
  await expect(failure).rejects.toBeInstanceOf(NetworkError);
  await expect(failure).rejects.toHaveProperty(
    'message',
    NETWORK_ERROR_MESSAGE,
  );
});

test('a cancelled request keeps its AbortError', async () => {
  const abort = new DOMException('The user aborted a request.', 'AbortError');
  await expect(apiCall(Promise.reject(abort))).rejects.toBe(abort);
});
