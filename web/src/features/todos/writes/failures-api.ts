import { ApiError } from '../../../api-client';

/** The task (or category) is no longer what the screen shows: changed or deleted elsewhere, so it must reload. */
export const isStale = (error: Error) =>
  error instanceof ApiError &&
  (error.status === 404 || error.code === 'todos.changed_meanwhile');

/** Someone saved the task (or category) after this screen read it: 409 `todos.changed_meanwhile`. */
export const isChangedMeanwhile = (error: Error) =>
  error instanceof ApiError && error.code === 'todos.changed_meanwhile';

/** What a form shows after a failed write: one message, and the messages of the fields the API rejected. */
export interface WriteFailure {
  message: string;
  fieldErrors: Record<string, string[]>;
  /** The Idempotency-Key was already used for other data; the next attempt needs a new one. */
  renewKey?: boolean;
}

/** How the messages name what was written: a task (with its steps) or a category. */
const subjects = {
  task: { changed: 'Справу змінили', gone: 'Цієї справи вже немає' },
  category: { changed: 'Категорію змінили', gone: 'Цієї категорії вже немає' },
} as const;

/**
 * Turns a failed write into Russian text (docs/rules.md, "API"); `action` names the failed step, `subject` what was
 * written.
 */
export function describeWriteError(
  error: Error,
  action = 'зберегти',
  subject: keyof typeof subjects = 'task',
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
  // A category's own 404 is `todos.category_not_found` too, but nothing is left to choose there.
  if (subject === 'category' && error.status === 404) return gone;
  switch (error.code) {
    case 'todos.changed_meanwhile':
      return {
        message: `${words.changed} в іншому місці — дані оновлено, перевірте й спробуйте знову.`,
        fieldErrors,
      };
    case 'todos.category_not_found':
      return {
        message: 'Цієї категорії вже немає: виберіть іншу.',
        fieldErrors: { ...fieldErrors, categoryId: ['Категорії вже немає'] },
      };
    case 'todos.category_name_taken':
      return {
        message: 'Категорія з такою назвою вже є.',
        fieldErrors: { ...fieldErrors, name: ['Така назва вже є'] },
      };
    case 'todos.step_not_found':
      return {
        message: 'Цього кроку вже немає: список оновлено.',
        fieldErrors,
      };
    case 'todos.too_many_steps':
      return { message: 'У справі не більше 50 кроків.', fieldErrors };
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
