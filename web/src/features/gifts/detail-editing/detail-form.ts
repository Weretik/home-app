import type { GiftDetailItem, Recipient } from '../data/gifts-api';
import type { GiftDetailRequest } from '../data/settings-writes-api';
import { checkLength, type FieldErrors } from '../editing/field-checks';

/** A «нюанс» as the form holds it: for whom, what («Кольцо») and its value («18,5–19,0»). */
export interface DetailFormValues {
  recipient: Recipient | '';
  label: string;
  value: string;
}

export const emptyDetail = (recipient?: Recipient): DetailFormValues => ({
  recipient: recipient ?? '',
  label: '',
  value: '',
});

export const fromDetail = (detail: GiftDetailItem): DetailFormValues => ({
  recipient: detail.recipient,
  label: detail.label,
  value: detail.value,
});

export const validateDetail = (values: DetailFormValues): FieldErrors => ({
  recipient: values.recipient ? undefined : 'Выберите, для кого',
  label: checkLength(values.label, 100, 'Введите, что это'),
  value: checkLength(values.value, 200, 'Введите значение'),
});

export const toDetailRequest = (
  values: DetailFormValues,
): GiftDetailRequest => ({
  recipient: values.recipient as Recipient,
  label: values.label.trim(),
  value: values.value.trim(),
});
