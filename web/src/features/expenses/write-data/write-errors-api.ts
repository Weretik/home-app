import { ApiError } from '../../../api-client';

/** What a form shows after a failed write: one message, and the messages of the fields the API rejected. */
export interface WriteFailure {
  message: string;
  fieldErrors: Record<string, string[]>;
  /** The Idempotency-Key was already used for other data; the next attempt needs a new one. */
  renewKey?: boolean;
}

/** Turns a failed write into Russian text for the form (docs/rules.md, "API"); `action` names the failed step. */
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
  if (error.code === 'expenses.changed_meanwhile')
    return {
      message:
        'Запис змінили в іншому місці — оновіть і спробуйте знову. Список уже оновлено.',
      fieldErrors,
    };
  if (error.status === 404)
    return {
      message: 'Цього запису вже немає: його видалили в іншому місці.',
      fieldErrors,
    };
  if (error.status === 409)
    return {
      message: 'Цей запис уже надсилали з іншими даними. Спробуйте ще раз.',
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
