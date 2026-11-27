import { ApiError } from '../../../api-client';

/** What a form or dialog shows after a failed write: one message, and the messages of the rejected fields. */
export interface WriteFailure {
  message: string;
  fieldErrors: Record<string, string[]>;
  /** The Idempotency-Key was already used for other data; the next attempt needs a new one. */
  renewKey?: boolean;
}

const staleText =
  'Подарунок або запис змінили в іншому місці — список оновлено, відкрийте знову.';

/** The item is no longer what the page shows (changed or deleted elsewhere), so its list must reload. */
export const isStale = (error: Error) =>
  error instanceof ApiError &&
  (error.status === 404 ||
    error.status === 412 ||
    error.code === 'gifts.changed_meanwhile');

/** Turns a failed Gifts write into Russian text (docs/rules.md, "API"); `action` names the failed step. */
export function describeWriteError(
  error: Error,
  action = 'зберегти',
): WriteFailure {
  if (!(error instanceof ApiError))
    return {
      message:
        'Немає зв’язку із сервером. Перевірте інтернет і спробуйте знову.',
      fieldErrors: {},
    };
  const fieldErrors = error.fieldErrors;
  if (error.status === 412 || error.code === 'gifts.changed_meanwhile')
    return { message: staleText, fieldErrors };
  if (error.status === 404)
    return {
      message: 'Цього вже немає: його видалили в іншому місці.',
      fieldErrors,
    };
  if (error.code === 'gifts.category_in_use')
    return {
      message: 'Ця категорія є в подарунків — спершу перенесіть їх в іншу.',
      fieldErrors,
    };
  if (error.code === 'gifts.category_name_taken')
    return {
      message: 'Така категорія вже є.',
      fieldErrors: { name: ['Така категорія вже є'] },
    };
  if (error.code === 'gifts.idempotency_key_reused')
    return {
      message: 'Це вже надсилали з іншими даними. Спробуйте ще раз.',
      fieldErrors,
      renewKey: true,
    };
  if (error.status === 429)
    return {
      message: 'Забагато змін поспіль. Зачекайте хвилину й повторіть.',
      fieldErrors,
    };
  if (error.status === 400)
    return { message: `Перевірте поля: ${error.message}`, fieldErrors };
  return { message: `Не вдалося ${action}: ${error.message}`, fieldErrors };
}
