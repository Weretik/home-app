/**
 * One key per attempt of a create that must not run twice (an order). Keep the key while retrying the same
 * data; take a new one after a success or when the data changes.
 */
export function newIdempotencyKey(): string {
  return crypto.randomUUID();
}
