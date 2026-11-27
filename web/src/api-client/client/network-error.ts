/** The text shown when a request never reached the server: no network, or the server did not answer. */
export const NETWORK_ERROR_MESSAGE =
  'Немає звʼязку з сервером. Перевірте інтернет і спробуйте ще раз.';

/**
 * A request that got no answer at all (the browser's `TypeError: Failed to fetch`). It carries a message ready to
 * show, so no screen ever shows the browser's own English text.
 */
export class NetworkError extends Error {
  constructor(options?: { cause?: unknown }) {
    super(NETWORK_ERROR_MESSAGE, options);
    this.name = 'NetworkError';
  }
}
