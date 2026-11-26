import type { LimitUsage, Payer, Person } from './summary-api';

/** Russian names of the contract's enums, worded like the Expenses pages. */
export const payerLabels: Record<Payer, string> = {
  Vitaliy: 'Виталий',
  Maria: 'Маша',
  Shared: 'Общее',
};

export const personLabels: Record<Person, string> = {
  Vitaliy: 'Виталий',
  Maria: 'Маша',
};

/** A limit's name as the table shows it: quarterly limits say so. */
export const limitName = (limit: Pick<LimitUsage, 'name' | 'period'>) =>
  limit.period === 'Quarter' ? `${limit.name} (квартал)` : limit.name;
