import { checkListSettings, listSettingsRequest } from './list-settings-form';

test('accepts whole numbers in range', () => {
  const values = { cartKeepHours: '3', frequentDays: '183' };
  expect(checkListSettings(values)).toEqual({});
  expect(listSettingsRequest(values)).toEqual({
    cartKeepHours: 3,
    frequentDays: 183,
  });
});

test('says what is wrong with each field', () => {
  expect(
    checkListSettings({ cartKeepHours: '100', frequentDays: '2,5' }),
  ).toEqual({
    cartKeepHours: 'Від 1 до 72',
    frequentDays: 'Введіть ціле число',
  });
  expect(
    checkListSettings({ cartKeepHours: '0', frequentDays: '731' }),
  ).toEqual({
    cartKeepHours: 'Від 1 до 72',
    frequentDays: 'Від 30 до 730',
  });
});
