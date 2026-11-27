import type { Department, DepartmentRequest } from './departments-api';

export interface DepartmentValues {
  name: string;
  icon: string;
}

export const DEPARTMENT_NAME_MAX = 40;
export const ICON_MAX = 8;

export const departmentValues = (
  department?: Department,
): DepartmentValues => ({
  name: department?.name ?? '',
  icon: department?.icon ?? '',
});

/** The messages under the fields; empty when the form can be sent. */
export function checkDepartment(values: DepartmentValues) {
  const errors: Partial<Record<keyof DepartmentValues, string>> = {};
  const name = values.name.trim();
  if (!name) errors.name = 'Введіть назву';
  else if (name.length > DEPARTMENT_NAME_MAX)
    errors.name = `Не довше ${DEPARTMENT_NAME_MAX} символів`;
  if (values.icon.trim().length > ICON_MAX) errors.icon = 'Один-два емодзі';
  return errors;
}

export const departmentRequest = (
  values: DepartmentValues,
): DepartmentRequest => ({
  name: values.name.trim(),
  icon: values.icon.trim() || null,
});
