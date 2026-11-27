import { useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../../api-client';
import { api } from '../../../platform/client/api';
import {
  ifMatch,
  useShoppingWrite,
  type Versioned,
} from '../data/item-writes-api';
import { shoppingKey } from '../data/shopping-api';

export type Department = Schema<'DepartmentItem'>;
export type DepartmentRequest = Schema<'UpdateShoppingDepartmentRequest'>;

/** The departments in store-walk order with their keyword and needed counts. */
export function useDepartments() {
  return useQuery({
    queryKey: [...shoppingKey, 'departments'],
    queryFn: ({ signal }) =>
      apiCall(api.GET('/api/shopping/departments', { signal })),
  });
}

export interface SaveDepartmentInput {
  body: DepartmentRequest;
  /** Set for a rename; a new department leaves it out and goes before «Другое». */
  edit?: Versioned;
  /** Sent with a create (one per opened form). */
  idempotencyKey: string;
}

/** Adds a department or changes its name and icon. */
export const useSaveDepartment = () =>
  useShoppingWrite(({ body, edit, idempotencyKey }: SaveDepartmentInput) =>
    edit
      ? apiCall(
          api.PUT('/api/shopping/departments/{id}', {
            params: ifMatch(edit),
            body,
          }),
        )
      : apiCall(
          api.POST('/api/shopping/departments', {
            params: { header: { 'Idempotency-Key': idempotencyKey } },
            body,
          }),
        ),
  );

/** Deletes a department; its items and keywords move to the fallback. */
export const useDeleteDepartment = () =>
  useShoppingWrite((row: Versioned) =>
    apiCall(
      api.DELETE('/api/shopping/departments/{id}', { params: ifMatch(row) }),
    ),
  );

/** Saves the whole store-walk order (every id once). */
export const useReorderDepartments = () =>
  useShoppingWrite((departmentIds: string[]) =>
    apiCall(
      api.PUT('/api/shopping/departments/order', { body: { departmentIds } }),
    ),
  );
