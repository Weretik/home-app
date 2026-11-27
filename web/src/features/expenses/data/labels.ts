import type { Payer, PaymentMethod, Person } from './expenses-api';
import type { LimitPeriod } from '../budget-data/budget-api';

/** Russian names of the contract's enums, in the order filters list them. */
export const payerLabels: Record<Payer, string> = {
  Vitaliy: 'Віталій',
  Maria: 'Маша',
  Shared: 'Спільне',
};

export const personLabels: Record<Person, string> = {
  Vitaliy: 'Віталій',
  Maria: 'Маша',
};

export const paymentMethodLabels: Record<PaymentMethod, string> = {
  Card: 'Картка',
  Cash: 'Готівка',
};

export const limitPeriodLabels: Record<LimitPeriod, string> = {
  Month: 'на місяць',
  Quarter: 'на квартал',
};
