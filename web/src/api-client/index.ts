export {
  createApiClient,
  type ApiClient,
  type ApiClientOptions,
} from './client/create-api-client';
export { apiCall } from './client/api-call';
export { ApiError } from './client/api-error';
export { newIdempotencyKey } from './client/idempotency-key';
export type { components, operations, paths } from './generated/api';

import type { components } from './generated/api';

/** Shorthand for a response or request schema of the API, for example `Schema<'ProductCard'>`. */
export type Schema<Name extends keyof components['schemas']> =
  components['schemas'][Name];
