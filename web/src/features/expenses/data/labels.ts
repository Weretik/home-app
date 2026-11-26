import type { Payer, PaymentMethod, Person } from './expenses-api';
import type { LimitPeriod } from '../budget-data/budget-api';

/** Russian names of the contract's enums, in the order filters list them. */
export const payerLabels: Record<Payer, string> = {
  Vitaliy: 'Виталий',
  Maria: 'Маша',
  Shared: 'Общее',
};

export const personLabels: Record<Person, string> = {
  Vitaliy: 'Виталий',
  Maria: 'Маша',
};

export const paymentMethodLabels: Record<PaymentMethod, string> = {
  Card: 'Карта',
  Cash: 'Наличные',
};

export const limitPeriodLabels: Record<LimitPeriod, string> = {
  Month: 'в месяц',
  Quarter: 'в квартал',
};
