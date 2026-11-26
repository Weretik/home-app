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
    super(problem?.detail ?? problem?.title ?? 'The request failed.');
    this.name = 'ApiError';
    this.status = status;
    this.code = problem?.code ?? `http_${status}`;
    this.fieldErrors = camelCaseKeys(problem?.errors ?? {});
  }
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
