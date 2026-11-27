/** What the sign-in screen says when the session ended by itself (FR-005). */
export const SESSION_EXPIRED_TEXT = 'Сесія закінчилась, увійдіть знову';

const byCode: Record<string, string> = {
  'identity.not_a_member':
    'Ця пошта не має доступу. Увійдіть акаунтом Віталія або Маші.',
  'identity.google_token_invalid':
    'Google не підтвердив вхід. Спробуйте ще раз.',
  'identity.not_configured': 'Вхід ще не налаштовано на сервері.',
};

const fallback = 'Не вдалося увійти. Спробуйте ще раз.';

interface Problem {
  status: number;
  code: string;
  message: string;
}

const isProblem = (error: unknown): error is Problem =>
  typeof error === 'object' &&
  error !== null &&
  typeof (error as Problem).status === 'number' &&
  typeof (error as Problem).code === 'string' &&
  typeof (error as Problem).message === 'string';

/**
 * The Ukrainian text of a failed sign-in (FR-003), by the problem code of the API's answer. Takes the error as
 * React Query gives it: an `ApiError` (status, code, message) or anything else, such as a network failure.
 */
export function signInErrorText(error: unknown): string {
  if (!isProblem(error)) return fallback;
  if (error.status === 429) return error.message;
  return byCode[error.code] ?? fallback;
}
