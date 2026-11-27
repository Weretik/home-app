import { ApiError } from '../../../api-client';

/** The row is no longer what the page shows (changed or deleted elsewhere), so the page must reload. */
export const isStale = (error: Error) =>
  error instanceof ApiError &&
  (error.status === 404 ||
    error.status === 412 ||
    error.code.endsWith('changed_meanwhile') ||
    error.code === 'shopping.department_order_incomplete');

/** Russian text of the problems a person can cause and fix (the API's own messages are English). */
const knownProblems: Record<string, string> = {
  'shopping.keyword_taken': 'Таке слово вже є.',
  'shopping.department_name_taken': 'Відділ з такою назвою вже є.',
  'shopping.fallback_department': 'Відділ за замовчуванням видалити не можна.',
  'shopping.nothing_to_add': 'Нічого додати: напишіть назву.',
  'shopping.too_many_items': 'Забагато за раз: не більше 50 товарів.',
  'shopping.department_order_incomplete':
    'Відділи змінилися в іншому місці. Список оновлено — спробуйте знову.',
};

/** What a form shows after a failed write: one message, and the messages of the fields the API rejected. */
export interface WriteFailure {
  message: string;
  fieldErrors: Record<string, string[]>;
  /** The Idempotency-Key was already used for other data; the next attempt needs a new one. */
  renewKey?: boolean;
}

/** Turns a failed write into Russian text (docs/rules.md, "API"); `action` names the failed step. */
export function describeWriteError(
  error: Error,
  action = 'зберегти',
): WriteFailure {
  if (!(error instanceof ApiError))
    return {
      message:
        'Немає зв’язку з сервером. Перевірте інтернет і спробуйте знову.',
      fieldErrors: {},
    };
  const fieldErrors = error.fieldErrors;
  if (isStale(error))
    return {
      message:
        error.status === 404
          ? 'Цього вже немає: видалили в іншому місці. Список оновлено.'
          : 'Це змінили в іншому місці. Список оновлено — спробуйте знову.',
      fieldErrors,
    };
  const known = knownProblems[error.code];
  if (known) return { message: known, fieldErrors: {} };
  if (error.code.endsWith('idempotency_key_reused'))
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
  if (error.status === 400 && Object.keys(fieldErrors).length > 0)
    return { message: 'Перевірте поля.', fieldErrors };
  return { message: `Не вдалося ${action}: ${error.message}`, fieldErrors };
}
