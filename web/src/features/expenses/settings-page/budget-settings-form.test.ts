import {
  budgetSettingsValues,
  optionalAmountError,
  toBudgetSettingsRequest,
  validateBudgetSettings,
} from './budget-settings-form';

test('zero shows as an empty field (none); the month starts at the current one in Kyiv', () => {
  expect(
    budgetSettingsValues(
      {
        version: 1,
        monthlyLimit: 0,
        monthlyLimits: [],
        savingsGoal: 100000,
        limitsFrom: '2026-01-01',
        limits: [],
      },
      new Date('2026-11-30T22:30:00Z'),
    ),
  ).toEqual({
    monthlyLimit: '',
    monthlyLimitFrom: '2026-12',
    savingsGoal: '100000',
  });
});

test('an empty amount is fine; a wrong or too big one is not', () => {
  expect(optionalAmountError('')).toBeUndefined();
  expect(optionalAmountError('0')).toBeUndefined();
  expect(optionalAmountError('много')).toBe(
    'Сума — число, не більше двох знаків після коми',
  );
  expect(optionalAmountError('999999999999')).toBe('Завелика сума');
  expect(
    validateBudgetSettings({
      monthlyLimit: 'x',
      monthlyLimitFrom: '2026-12',
      savingsGoal: '',
    }),
  ).toEqual({
    monthlyLimit: 'Сума — число, не більше двох знаків після коми',
    monthlyLimitFrom: undefined,
    savingsGoal: undefined,
  });
});

test('the month is needed', () => {
  expect(
    validateBudgetSettings({
      monthlyLimit: '',
      monthlyLimitFrom: '',
      savingsGoal: '',
    }).monthlyLimitFrom,
  ).toBe('Виберіть місяць');
});

const inForce = { monthlyLimit: 50000, monthlyLimitFrom: '2026-12' };

test('an empty field is sent as 0, the month as its first day', () => {
  expect(
    toBudgetSettingsRequest(
      {
        monthlyLimit: '',
        monthlyLimitFrom: '2027-01',
        savingsGoal: '120 000',
      },
      inForce,
    ),
  ).toEqual({
    monthlyLimit: 0,
    savingsGoal: 120000,
    monthlyLimitFrom: '2027-01-01',
  });
});

test('a changed limit is sent from the default month', () => {
  expect(
    toBudgetSettingsRequest(
      { monthlyLimit: '55 000', monthlyLimitFrom: '2026-12', savingsGoal: '' },
      inForce,
    ),
  ).toEqual({
    monthlyLimit: 55000,
    savingsGoal: 0,
    monthlyLimitFrom: '2026-12-01',
  });
});

test('a goal-only save sends no month, so later steps stay', () => {
  const request = toBudgetSettingsRequest(
    {
      monthlyLimit: '50 000,00',
      monthlyLimitFrom: '2026-12',
      savingsGoal: '120000',
    },
    inForce,
  );
  expect(request).toEqual({ monthlyLimit: 50000, savingsGoal: 120000 });
  expect(request).not.toHaveProperty('monthlyLimitFrom');
});

test('the same limit from another month is sent', () => {
  expect(
    toBudgetSettingsRequest(
      { monthlyLimit: '50000', monthlyLimitFrom: '2027-04', savingsGoal: '' },
      inForce,
    ),
  ).toMatchObject({ monthlyLimitFrom: '2027-04-01' });
});
