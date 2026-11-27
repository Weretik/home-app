import {
  budgetSettingsValues,
  optionalAmountError,
  toBudgetSettingsRequest,
  validateBudgetSettings,
} from './budget-settings-form';

test('zero shows as an empty field (none)', () => {
  expect(
    budgetSettingsValues({
      version: 1,
      monthlyLimit: 0,
      savingsGoal: 100000,
      limitsFrom: '2026-01-01',
      limits: [],
    }),
  ).toEqual({ monthlyLimit: '', savingsGoal: '100000' });
});

test('an empty amount is fine; a wrong or too big one is not', () => {
  expect(optionalAmountError('')).toBeUndefined();
  expect(optionalAmountError('0')).toBeUndefined();
  expect(optionalAmountError('много')).toBe(
    'Сума — число, не більше двох знаків після коми',
  );
  expect(optionalAmountError('999999999999')).toBe('Завелика сума');
  expect(
    validateBudgetSettings({ monthlyLimit: 'x', savingsGoal: '' }),
  ).toEqual({
    monthlyLimit: 'Сума — число, не більше двох знаків після коми',
    savingsGoal: undefined,
  });
});

test('an empty field is sent as 0', () => {
  expect(
    toBudgetSettingsRequest({ monthlyLimit: '', savingsGoal: '120 000' }),
  ).toEqual({ monthlyLimit: 0, savingsGoal: 120000 });
});
