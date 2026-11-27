/** Days the expense list shows instead of a month: both «YYYY-MM-DD», both included. */
export interface Period {
  from: string;
  to: string;
}

/** The API's limit on a period (ListExpensesValidator.MaxPeriodDays). */
export const MAX_PERIOD_DAYS = 366;

const isoPattern = /^\d{4}-\d{2}-\d{2}$/;

/** The day number of «YYYY-MM-DD», or undefined when it is not a real date between 2000 and 2100. */
function dayNumber(text: string): number | undefined {
  if (!isoPattern.test(text)) return undefined;
  const [year, month, day] = text.split('-').map(Number) as [
    number,
    number,
    number,
  ];
  const time = Date.UTC(year, month - 1, day);
  const date = new Date(time);
  const real =
    date.getUTCFullYear() === year &&
    date.getUTCMonth() === month - 1 &&
    date.getUTCDate() === day;
  return real && year >= 2000 && year <= 2100 ? time / 86_400_000 : undefined;
}

/** Why the period cannot be shown, in Russian; undefined when the list can load it. */
export function periodError({ from, to }: Period): string | undefined {
  if (!from || !to) return 'Виберіть обидві дати';
  const start = dayNumber(from);
  const end = dayNumber(to);
  if (start === undefined || end === undefined)
    return 'Дати між 2000 і 2100 роком';
  if (start > end) return 'Дата «з» пізніше за дату «по»';
  if (end - start + 1 > MAX_PERIOD_DAYS)
    return `Період не довше ${MAX_PERIOD_DAYS} днів`;
  return undefined;
}
