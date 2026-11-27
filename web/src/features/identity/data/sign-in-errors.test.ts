import { signInErrorText } from './sign-in-errors';

const problem = (status: number, code: string, message = 'API text') => ({
  status,
  code,
  message,
});

test.each([
  [
    problem(403, 'identity.not_a_member'),
    'Ця пошта не має доступу. Увійдіть акаунтом Віталія або Маші.',
  ],
  [
    problem(401, 'identity.google_token_invalid'),
    'Google не підтвердив вхід. Спробуйте ще раз.',
  ],
  [
    problem(409, 'identity.not_configured'),
    'Вхід ще не налаштовано на сервері.',
  ],
  [
    problem(429, 'http_429', 'Забагато спроб, зачекайте хвилину.'),
    'Забагато спроб, зачекайте хвилину.',
  ],
  [problem(500, 'http_500'), 'Не вдалося увійти. Спробуйте ще раз.'],
  [problem(400, 'validation'), 'Не вдалося увійти. Спробуйте ще раз.'],
  [new TypeError('Failed to fetch'), 'Не вдалося увійти. Спробуйте ще раз.'],
  [null, 'Не вдалося увійти. Спробуйте ще раз.'],
])('%o → %s', (error, text) => {
  expect(signInErrorText(error)).toBe(text);
});
