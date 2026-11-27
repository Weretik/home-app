import {
  recordFormValues,
  toRecordRequest,
  validateRecord,
} from './record-form';

test('records a plan today, by card, without a comment', () => {
  expect(recordFormValues(new Date(2026, 10, 26))).toEqual({
    date: '2026-11-26',
    paymentMethod: 'Card',
    comment: '',
  });
});

test('checks the date and the comment', () => {
  expect(
    validateRecord({ date: '', paymentMethod: 'Card', comment: '' }).date,
  ).toBe('Виберіть дату');
  expect(
    validateRecord({
      date: '2026-11-26',
      paymentMethod: 'Cash',
      comment: 'x'.repeat(501),
    }).comment,
  ).toBeDefined();
});

test('sends an empty comment as null', () => {
  expect(
    toRecordRequest({
      date: '2026-11-26',
      paymentMethod: 'Cash',
      comment: '  ',
    }),
  ).toEqual({ date: '2026-11-26', paymentMethod: 'Cash', comment: null });
});
