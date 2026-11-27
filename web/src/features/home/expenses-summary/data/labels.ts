import type { LimitUsage, Payer, Person } from './summary-api';

/** Russian names of the contract's enums, worded like the Expenses pages. */
export const payerLabels: Record<Payer, string> = {
  Vitaliy: 'Віталій',
  Maria: 'Маша',
  Shared: 'Спільне',
};

export const personLabels: Record<Person, string> = {
  Vitaliy: 'Віталій',
  Maria: 'Маша',
};

/** A limit's name as the table shows it: quarterly limits say so. */
export const limitName = (limit: Pick<LimitUsage, 'name' | 'period'>) =>
  limit.period === 'Quarter' ? `${limit.name} (квартал)` : limit.name;
