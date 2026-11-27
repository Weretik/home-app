/**
 * The RFC 9457 problem body. The backend adds `code` and `traceId` to the standard fields, and `errors` by field for
 * a validation failure. Written by hand because not every contract version lists it as a schema.
 */
interface Problem {
  title?: string | null;
  detail?: string | null;
  status?: number | null;
  errors?: Record<string, string[]>;
  code?: string;
  traceId?: string;
}

/**
 * A failed API call. It carries the backend's RFC 9457 problem: `code` is the stable error code to branch on,
 * `message` is ready to show, and `fieldErrors` holds the messages of a validation failure by field.
 */
export class ApiError extends Error {
  readonly status: number;
  readonly code: string;
  readonly fieldErrors: Record<string, string[]>;

  constructor(status: number, problem: Partial<Problem> | undefined) {
    super(readableMessage(status, problem));
    this.name = 'ApiError';
    this.status = status;
    this.code = problem?.code ?? `http_${status}`;
    this.fieldErrors = camelCaseKeys(problem?.errors ?? {});
  }
}

/** Backend messages are Ukrainian; a framework's default text (English) is replaced by one by status. */
const hasCyrillic = (text: string | null | undefined): text is string =>
  Boolean(text && /[А-Яа-яІіЇїЄєҐґ]/.test(text));

/** The text for an answer whose problem has no Ukrainian message. */
export function statusMessage(status: number): string {
  if (status === 401) return 'Сесія закінчилась. Увійдіть ще раз.';
  if (status === 403) return 'Немає доступу.';
  if (status === 404) return 'Цього вже немає.';
  if (status === 409 || status === 412)
    return 'Це вже змінили в іншому місці. Оновіть і спробуйте ще раз.';
  if (status === 429) return 'Забагато запитів поспіль. Зачекайте хвилину.';
  if (status >= 500) return 'Сервер зараз не відповідає. Спробуйте пізніше.';
  return 'Не вдалося виконати запит. Спробуйте ще раз.';
}

function readableMessage(
  status: number,
  problem: Partial<Problem> | undefined,
): string {
  if (hasCyrillic(problem?.detail)) return problem.detail;
  if (hasCyrillic(problem?.title)) return problem.title;
  return statusMessage(status);
}

/** The backend names fields like its C# properties (`CustomerName`, `Lines[0].Quantity`); forms use camelCase. */
function camelCaseKeys(
  errors: Record<string, string[]>,
): Record<string, string[]> {
  const camel = (key: string) =>
    key
      .split('.')
      .map((part) => part.charAt(0).toLowerCase() + part.slice(1))
      .join('.');
  return Object.fromEntries(
    Object.entries(errors).map(([key, messages]) => [camel(key), messages]),
  );
}
