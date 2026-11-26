import { ApiError } from './api-error';

interface FetchResult<T> {
  data?: T;
  error?: unknown;
  response: Response;
}

/**
 * Unwraps an openapi-fetch call: the data of a success, or an {@link ApiError} for any failure. Data-access
 * code uses it so screens deal with one error type.
 */
export async function apiCall<T>(request: Promise<FetchResult<T>>): Promise<T> {
  const { data, error, response } = await request;
  if (!response.ok) {
    throw new ApiError(
      response.status,
      error as ConstructorParameters<typeof ApiError>[1],
    );
  }
  return data as T;
}
