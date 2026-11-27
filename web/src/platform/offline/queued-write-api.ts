import type { QueryClient } from '@tanstack/react-query';
import { ApiError, apiCall } from '../../api-client';
import { markOffline } from '../client/connectivity';
import { api } from '../client/api';
import { moduleKeys } from '../client/query-client';
import { recordRefused } from './network-status';
import {
  queuedWriteKey,
  type QueuedRequest,
  type QueuedVariables,
} from './queued-request';

type LooseRequest = (
  method: string,
  path: string,
  init: { body?: unknown; headers?: Record<string, string> },
) => Promise<{ data?: unknown; error?: unknown; response: Response }>;

// The path is already concrete (no `{id}` left), which the typed signature cannot express.
const call = api.request as unknown as LooseRequest;

const send = (request: QueuedRequest, version?: number) =>
  apiCall(
    call(request.method, request.path, {
      body: request.body,
      headers: {
        ...request.headers,
        ...(version === undefined ? {} : { 'If-Match': String(version) }),
      },
    }),
  );

const isGone = (error: unknown) =>
  error instanceof ApiError && (error.status === 404 || error.status === 410);
const isStale = (error: unknown) =>
  error instanceof ApiError && (error.status === 409 || error.status === 412);

/** The record's version now, read from `refetch`; undefined when the answer carries none. */
async function currentVersion(refetch: string): Promise<number | undefined> {
  const current = await apiCall(call('get', refetch, {}));
  const version = (current as { version?: unknown } | undefined)?.version;
  return typeof version === 'number' ? version : undefined;
}

/**
 * Sends a queued write. A deleted record stays deleted (`404`/`410` count as done, nothing is recreated); a record
 * changed meanwhile (`409`/`412`) is read again and the write is sent once more with its new version (last write
 * wins); any other refusal throws the `ApiError`; a network failure throws as it is, so the write is retried.
 */
export async function sendQueuedRequest(
  request: QueuedRequest,
): Promise<unknown> {
  let refused: unknown;
  try {
    return await send(request);
  } catch (error) {
    if (isGone(error)) return undefined;
    if (!isStale(error) || !request.refetch) throw error;
    refused = error;
  }
  try {
    const version = await currentVersion(request.refetch);
    if (version === undefined) throw refused;
    return await send(request, version);
  } catch (error) {
    if (isGone(error)) return undefined;
    throw error;
  }
}

/**
 * Retries a network failure until it passes (counting the app offline, so the write pauses until the network is
 * back), a server error three times; a refusal (4xx) never.
 */
export function retryQueuedWrite(failureCount: number, error: Error): boolean {
  if (!(error instanceof ApiError)) {
    markOffline();
    return true;
  }
  return error.status >= 500 && failureCount < 3;
}

/**
 * The defaults of every queued write, also of one restored from IndexedDB after a restart, when only the request is
 * left: send it, pause while offline, refresh its module, and record a refusal for the snackbar.
 */
export function registerQueuedWrites(queryClient: QueryClient) {
  queryClient.setMutationDefaults(queuedWriteKey, {
    mutationFn: (variables: QueuedVariables) =>
      sendQueuedRequest(variables.request),
    networkMode: 'online',
    retry: retryQueuedWrite,
    onError: (_error: Error, variables: QueuedVariables) =>
      recordRefused(variables.request.label),
    onSettled: (
      _data: unknown,
      _error: Error | null,
      variables: QueuedVariables,
    ) =>
      queryClient.invalidateQueries({
        queryKey: moduleKeys[variables.request.module],
      }),
  });
}
