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
  'shopping.keyword_taken': 'Такое слово уже есть.',
  'shopping.department_name_taken': 'Отдел с таким названием уже есть.',
  'shopping.fallback_department': 'Отдел по умолчанию удалить нельзя.',
  'shopping.nothing_to_add': 'Нечего добавить: напишите название.',
  'shopping.too_many_items': 'Слишком много за раз: не больше 50 товаров.',
  'shopping.department_order_incomplete':
    'Отделы изменились в другом месте. Список обновлён — попробуйте снова.',
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
  action = 'сохранить',
): WriteFailure {
  if (!(error instanceof ApiError))
    return {
      message: 'Нет связи с сервером. Проверьте интернет и попробуйте снова.',
      fieldErrors: {},
    };
  const fieldErrors = error.fieldErrors;
  if (isStale(error))
    return {
      message:
        error.status === 404
          ? 'Этого уже нет: удалили в другом месте. Список обновлён.'
          : 'Это изменили в другом месте. Список обновлён — попробуйте снова.',
      fieldErrors,
    };
  const known = knownProblems[error.code];
  if (known) return { message: known, fieldErrors: {} };
  if (error.code.endsWith('idempotency_key_reused'))
    return {
      message: 'Это уже отправляли с другими данными. Попробуйте ещё раз.',
      fieldErrors,
      renewKey: true,
    };
  if (error.status === 429)
    return {
      message: 'Слишком много изменений подряд. Подождите минуту и повторите.',
      fieldErrors,
    };
  if (error.status === 400 && Object.keys(fieldErrors).length > 0)
    return { message: 'Проверьте поля.', fieldErrors };
  return { message: `Не удалось ${action}: ${error.message}`, fieldErrors };
}
