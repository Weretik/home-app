import { getJson } from '../../platform/api-client';

export type Health = {
  status: 'Healthy' | 'Degraded' | 'Unhealthy';
  version: string;
  environment: string;
  checks: Record<string, string>;
};

export function getHealth(signal?: AbortSignal) {
  return getJson<Health>('/health', { acceptStatuses: [503], signal });
}
