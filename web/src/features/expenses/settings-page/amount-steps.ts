import { locale } from '../../../platform/locale';
import type { AmountStep } from '../budget-data/budget-api';
import { formatAmount } from '../data/formatting';
import { kyivMonthText } from '../data/month';

/** The month in the genitive, as a day writes it («1 грудня»): only the month part is kept. */
const dayAndMonth = new Intl.DateTimeFormat(locale, {
  day: 'numeric',
  month: 'long',
});

/**
 * The step in force in `month` («YYYY-MM») and the one before it: the last step whose `from` is not after the month.
 * Undefined when no step is in force yet or it is the first one. `steps` come oldest first, as the API sends them.
 */
export function stepsAround(
  steps: AmountStep[],
  month: string,
): { current: AmountStep; before: AmountStep } | undefined {
  let index = -1;
  steps.forEach((step, at) => {
    if (step.from.slice(0, 7) <= month) index = at;
  });
  const current = steps[index];
  const before = steps[index - 1];
  return current && before ? { current, before } : undefined;
}

/** «з грудня 2026» from a contract day («2026-12-01»). */
export function formatSince(day: string): string {
  const [year = 2000, month = 1] = day.split('-').map(Number);
  const name =
    dayAndMonth
      .formatToParts(new Date(year, month - 1, 1))
      .find((part) => part.type === 'month')?.value ?? '';
  return `з ${name} ${year}`;
}

/**
 * The caption under a limit's amount: since when today's amount applies and the amount before it, «з грудня 2026 ·
 * раніше 7 000,00 ₴». Undefined while the limit has had one amount only (web SDD 006, R-WLA-004).
 */
export function earlierAmountCaption(
  steps: AmountStep[],
  now: Date = new Date(),
): string | undefined {
  const around = stepsAround(steps, kyivMonthText(now));
  return (
    around &&
    `${formatSince(around.current.from)} · раніше ${formatAmount(around.before.amount)}`
  );
}

/** The first step that starts after `month` («YYYY-MM»): a change saved ahead, not in force yet. */
export function nextStep(
  steps: AmountStep[],
  month: string,
): AmountStep | undefined {
  return steps.find((step) => step.from.slice(0, 7) > month);
}

/**
 * The caption of a change saved ahead, «з квітня 2027 — 6 000,00 ₴»: the next step after today's month. Undefined
 * when nothing is scheduled (web SDD 006, R-WLA-007).
 */
export function nextAmountCaption(
  steps: AmountStep[],
  now: Date = new Date(),
): string | undefined {
  const next = nextStep(steps, kyivMonthText(now));
  return next && `${formatSince(next.from)} — ${formatAmount(next.amount)}`;
}

/**
 * The hint under the monthly limit field: the earlier amount and the scheduled one when there are any, then what an
 * empty field means (R-WLA-004, R-WLA-007).
 */
export function monthlyLimitHint(
  steps: AmountStep[],
  now: Date = new Date(),
): string {
  const captions = [
    earlierAmountCaption(steps, now),
    nextAmountCaption(steps, now),
  ].filter(Boolean);
  return captions.length > 0
    ? `${captions.join('; ')}; порожньо — без ліміту`
    : 'Порожньо — без ліміту';
}
