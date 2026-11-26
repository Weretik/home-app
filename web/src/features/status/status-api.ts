import { useQuery } from '@tanstack/react-query';
import { apiCall, type Schema } from '../../api-client';
import { api } from '../../platform/api';

export type SystemStatus = Schema<'SystemStatus'>;

export function useSystemStatus() {
  return useQuery({
    queryKey: ['system', 'status'],
    queryFn: ({ signal }) => apiCall(api.GET('/api/system/status', { signal })),
  });
}
