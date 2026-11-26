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
  action = 'сохранить',
): WriteFailure {
  if (!(error instanceof ApiError))
    return {
      message: 'Нет связи с сервером. Проверьте интернет и попробуйте снова.',
      fieldErrors: {},
    };
  const fieldErrors = error.fieldErrors;
  if (error.code === 'expenses.changed_meanwhile')
    return {
      message:
        'Запись изменили в другом месте — обновите и попробуйте снова. Список уже обновлён.',
      fieldErrors,
    };
  if (error.status === 404)
    return {
      message: 'Этой записи уже нет: её удалили в другом месте.',
      fieldErrors,
    };
  if (error.status === 409)
    return {
      message:
        'Эту запись уже отправляли с другими данными. Попробуйте ещё раз.',
      fieldErrors,
      renewKey: true,
    };
  if (error.status === 429)
    return {
      message: 'Слишком много изменений подряд. Подождите минуту и повторите.',
      fieldErrors,
    };
  if (error.status === 400)
    return { message: `Проверьте поля: ${error.message}`, fieldErrors };
  return { message: `Не удалось ${action}: ${error.message}`, fieldErrors };
}
