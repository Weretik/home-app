import { ApiError } from '../../../api-client';

/** The note (or category) is no longer what the screen shows: changed or deleted elsewhere, so it must reload. */
export const isStale = (error: Error) =>
  error instanceof ApiError &&
  (error.status === 404 || error.code === 'notes.changed_meanwhile');

/** Someone saved the note (or category) after this screen read it: 409 `notes.changed_meanwhile`. */
export const isChangedMeanwhile = (error: Error) =>
  error instanceof ApiError && error.code === 'notes.changed_meanwhile';

/** The note itself is gone: 404 `notes.note_not_found`. */
export const isNoteGone = (error: Error) =>
  error instanceof ApiError && error.code === 'notes.note_not_found';

/** What a form shows after a failed write: one message, and the messages of the fields the API rejected. */
export interface WriteFailure {
  message: string;
  fieldErrors: Record<string, string[]>;
  /** The Idempotency-Key was already used for other data; the next attempt needs a new one. */
  renewKey?: boolean;
}

const subjects = {
  note: { changed: 'Нотатку змінили', gone: 'Цієї нотатки вже немає' },
  category: { changed: 'Категорію змінили', gone: 'Цієї категорії вже немає' },
} as const;

/** Turns a failed write into Ukrainian text (docs/rules.md, "API"); `action` names the failed step. */
export function describeWriteError(
  error: Error,
  action = 'зберегти',
  subject: keyof typeof subjects = 'note',
): WriteFailure {
  const words = subjects[subject];
  if (!(error instanceof ApiError))
    return {
      message:
        'Немає зв’язку з сервером. Перевірте інтернет і спробуйте знову.',
      fieldErrors: {},
    };
  const fieldErrors = error.fieldErrors;
  const gone = {
    message: `${words.gone}: її видалили в іншому місці.`,
    fieldErrors,
  };
  if (subject === 'category' && error.status === 404) return gone;
  switch (error.code) {
    case 'notes.changed_meanwhile':
      return {
        message: `${words.changed} в іншому місці — дані оновлено, перевірте й спробуйте знову.`,
        fieldErrors,
      };
    case 'notes.category_not_found':
      return {
        message: 'Цієї категорії вже немає: виберіть іншу.',
        fieldErrors: { ...fieldErrors, categoryId: ['Категорії вже немає'] },
      };
    case 'notes.category_name_taken':
      return {
        message: 'Категорія з такою назвою вже є.',
        fieldErrors: { ...fieldErrors, name: ['Така назва вже є'] },
      };
  }
  if (error.status === 404) return gone;
  if (error.status === 409)
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

/** The first message of each field the API rejected, for the form's fields. */
export const firstMessages = (failure: WriteFailure | undefined) =>
  Object.fromEntries(
    Object.entries(failure?.fieldErrors ?? {}).map(([name, messages]) => [
      name,
      messages[0],
    ]),
  ) as Record<string, string | undefined>;
