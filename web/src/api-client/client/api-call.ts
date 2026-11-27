import { ApiError } from './api-error';
import { NetworkError } from './network-error';

interface FetchResult<T> {
  data?: T;
  error?: unknown;
  response: Response;
}

/** A cancelled request (React Query's `signal`) is not a network failure and keeps its own error. */
const isAbort = (error: unknown) =>
  error instanceof DOMException && error.name === 'AbortError';

/**
 * Unwraps an openapi-fetch call: the data of a success, an {@link ApiError} for an error answer, or a
 * {@link NetworkError} when no answer came. Data-access code uses it so screens deal with known error types.
 */
export async function apiCall<T>(request: Promise<FetchResult<T>>): Promise<T> {
  let result: FetchResult<T>;
  try {
    result = await request;
  } catch (failure) {
    if (isAbort(failure)) throw failure;
    throw new NetworkError({ cause: failure });
  }
  const { data, error, response } = result;
  if (!response.ok) {
    throw new ApiError(
      response.status,
      error as ConstructorParameters<typeof ApiError>[1],
    );
  }
  return data as T;
}
