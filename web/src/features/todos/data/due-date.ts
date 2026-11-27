/**
 * Due dates of tasks: today in Kyiv (R-013), day arithmetic and the short Ukrainian labels («сьогодні», «завтра»,
 * «пн 30.11»). Dates are contract days («2026-11-30») and are compared as strings; arithmetic goes through UTC so no
 * time zone or daylight saving shift moves a day.
 */

const kyivDay = new Intl.DateTimeFormat('en-CA', {
  timeZone: 'Europe/Kyiv',
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
});

const weekdays = ['нд', 'пн', 'вт', 'ср', 'чт', 'пт', 'сб'] as const;

/** Today in Europe/Kyiv as «YYYY-MM-DD», the day the API calls today. */
export function kyivToday(now: Date = new Date()): string {
  return kyivDay.format(now);
}

const toUtc = (day: string) => {
  const [year = 2000, month = 1, date = 1] = day.split('-').map(Number);
  return new Date(Date.UTC(year, month - 1, date));
};

const fromUtc = (date: Date) => date.toISOString().slice(0, 10);

/** The day `days` after `day` (negative goes back). */
export function addDays(day: string, days: number): string {
  const date = toUtc(day);
  date.setUTCDate(date.getUTCDate() + days);
  return fromUtc(date);
}

/** The coming weekend's Saturday; today on a Saturday or a Sunday. */
export function weekendOf(today: string): string {
  const weekday = toUtc(today).getUTCDay();
  if (weekday === 6 || weekday === 0) return today;
  return addDays(today, 6 - weekday);
}

/** Whether an open task's due date has passed (before today in Kyiv). */
export const isOverdue = (dueDate: string | null | undefined, today: string) =>
  Boolean(dueDate && dueDate < today);

/** «сьогодні», «завтра», «вчора», else «пн 30.11» (with the year when it is not this year: «пн 30.11.2027»). */
export function formatDue(day: string, today: string): string {
  if (day === today) return 'сьогодні';
  if (day === addDays(today, 1)) return 'завтра';
  if (day === addDays(today, -1)) return 'вчора';
  return formatShortDay(day, today);
}

/** «пн 30.11», or «пн 30.11.2027» outside the year of `today`. */
export function formatShortDay(day: string, today: string): string {
  const [year, month, date] = day.split('-');
  const weekday = weekdays[toUtc(day).getUTCDay()];
  const sameYear = year === today.slice(0, 4);
  return `${weekday} ${date}.${month}${sameYear ? '' : `.${year}`}`;
}

/** An instant from the API («…Z») as a Kyiv day, for the archive's completion date. */
export function kyivDayOf(instant: string): string {
  return kyivToday(new Date(instant));
}
