import { ApiError } from '../../../api-client';

/** What a form or dialog shows after a failed write: one message, and the messages of the rejected fields. */
export interface WriteFailure {
  message: string;
  fieldErrors: Record<string, string[]>;
  /** The Idempotency-Key was already used for other data; the next attempt needs a new one. */
  renewKey?: boolean;
}

const staleText =
  'Подарок или запись изменили в другом месте — список обновлён, откройте снова.';

/** The item is no longer what the page shows (changed or deleted elsewhere), so its list must reload. */
export const isStale = (error: Error) =>
  error instanceof ApiError &&
  (error.status === 404 ||
    error.status === 412 ||
    error.code === 'gifts.changed_meanwhile');

/** Turns a failed Gifts write into Russian text (docs/rules.md, "API"); `action` names the failed step. */
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
  if (error.status === 412 || error.code === 'gifts.changed_meanwhile')
    return { message: staleText, fieldErrors };
  if (error.status === 404)
    return {
      message: 'Этого уже нет: его удалили в другом месте.',
      fieldErrors,
    };
  if (error.code === 'gifts.category_in_use')
    return {
      message:
        'Эта категория есть у подарков — сначала перенесите их в другую.',
      fieldErrors,
    };
  if (error.code === 'gifts.category_name_taken')
    return {
      message: 'Такая категория уже есть.',
      fieldErrors: { name: ['Такая категория уже есть'] },
    };
  if (error.code === 'gifts.idempotency_key_reused')
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
  if (error.status === 400)
    return { message: `Проверьте поля: ${error.message}`, fieldErrors };
  return { message: `Не удалось ${action}: ${error.message}`, fieldErrors };
}
