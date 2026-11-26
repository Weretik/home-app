/** The part of the quick-add text that is being typed now, as the parser splits items (`,`, `;`, new line). */
export interface NameSegment {
  /** Where the segment starts in the text: just after the last separator, or 0. */
  start: number;
  /** The segment without leading spaces, bullets or dashes and trailing spaces; what suggestions are asked for. */
  query: string;
}

const SEPARATORS = /[,;\n]/g;
const LEADING = /^[\s\-–—•*]+/;

/** «молоко, хле» → `{ start: 7, query: 'хле' }`; «молоко, » → an empty query (nothing to suggest). */
export function currentSegment(text: string): NameSegment {
  let start = 0;
  for (const match of text.matchAll(SEPARATORS)) start = match.index + 1;
  return { start, query: text.slice(start).replace(LEADING, '').trimEnd() };
}

/**
 * The text with the segment being typed replaced by a chosen name; what comes before it stays as typed, and one space
 * follows the separator. «молоко,хле» → «молоко, Хлеб»; «хле» → «Хлеб».
 */
export function replaceSegment(text: string, name: string): string {
  const { start } = currentSegment(text);
  const before = text.slice(0, start);
  if (start === 0) return name;
  return before.endsWith('\n') ? `${before}${name}` : `${before} ${name}`;
}
