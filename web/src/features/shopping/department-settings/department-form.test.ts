import {
  checkDepartment,
  DEPARTMENT_NAME_MAX,
  departmentRequest,
  departmentValues,
  ICON_MAX,
} from './department-form';

test('starts empty for a new department and from the row for an edit', () => {
  expect(departmentValues()).toEqual({ name: '', icon: '' });
  expect(
    departmentValues({ name: 'Бакалея', icon: '🍝' } as Parameters<
      typeof departmentValues
    >[0]),
  ).toEqual({ name: 'Бакалея', icon: '🍝' });
});

test('needs a name within the limit and a short icon', () => {
  expect(checkDepartment({ name: '  ', icon: '' })).toEqual({
    name: 'Введите название',
  });
  expect(
    checkDepartment({
      name: 'я'.repeat(DEPARTMENT_NAME_MAX + 1),
      icon: '🍝'.repeat(ICON_MAX),
    }),
  ).toEqual({
    name: `Не длиннее ${DEPARTMENT_NAME_MAX} символов`,
    icon: 'Один-два эмодзи',
  });
  expect(checkDepartment({ name: 'Бакалея', icon: '🍝' })).toEqual({});
});

test('trims the fields and sends no icon when it is empty', () => {
  expect(departmentRequest({ name: ' Бакалея ', icon: ' ' })).toEqual({
    name: 'Бакалея',
    icon: null,
  });
});
