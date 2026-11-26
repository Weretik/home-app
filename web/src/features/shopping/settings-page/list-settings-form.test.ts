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
    cartKeepHours: 'От 1 до 72',
    frequentDays: 'Введите целое число',
  });
  expect(
    checkListSettings({ cartKeepHours: '0', frequentDays: '731' }),
  ).toEqual({
    cartKeepHours: 'От 1 до 72',
    frequentDays: 'От 30 до 730',
  });
});
