import { useMutation, useQueryClient } from '@tanstack/react-query';
import { moduleKeys } from '../client/query-client';
import { isOnline, recordRefused } from './network-status';
import {
  queuedWriteKey,
  type QueuedRequest,
  type QueuedVariables,
} from './queued-request';
import { retryQueuedWrite, sendQueuedRequest } from './queued-write-api';

/** Requests made while offline, so their refusal is shown in the top bar too (the open screen may be gone by then). */
const wasQueued = new WeakSet<QueuedRequest>();

export interface QueuedWriteOptions<Input, Output, Context> {
  /** Appended to `['queued-write']`, for a feature that filters its own pending writes (the shopping tick). */
  key?: readonly unknown[];
  /** The optimistic change; its return value is the context of the callbacks below. */
  onMutate?: (input: Input) => Promise<Context> | Context;
  onError?: (error: Error, input: Input, context: Context | undefined) => void;
  onSuccess?: (data: Output | undefined, input: Input) => void;
  /** Replaces the default, which refreshes the request's module (ADR 0009). */
  onSettled?: (input: Input) => Promise<unknown> | void;
}

/**
 * A write that waits for the network (web ADR 0022): the feature calls `mutate(input)` with its own input; the
 * mutation is persisted with the serialisable request `toRequest(input)` so it is sent also after a restart. Offline
 * it pauses; `isSaving` is false then, so the screen does not wait on it. A refusal of a write made offline is also
 * recorded for the top bar's snackbar.
 */
export function useQueuedWrite<Input, Output = unknown, Context = unknown>(
  toRequest: (input: Input) => QueuedRequest,
  options: QueuedWriteOptions<Input, Output, Context> = {},
) {
  const queryClient = useQueryClient();
  const mutation = useMutation<
    Output | undefined,
    Error,
    QueuedVariables<Input>,
    Context | undefined
  >({
    mutationKey: [...queuedWriteKey, ...(options.key ?? [])],
    // The answer's body; undefined when the record was gone (a deleted record stays deleted).
    mutationFn: ({ request }) =>
      sendQueuedRequest(request) as Promise<Output | undefined>,
    networkMode: 'online',
    retry: retryQueuedWrite,
    onMutate: ({ input }) => options.onMutate?.(input),
    onError: (error, { request, input }, context) => {
      if (wasQueued.has(request)) recordRefused(request.label);
      options.onError?.(error, input, context);
    },
    onSuccess: (data, { input }) => options.onSuccess?.(data, input),
    onSettled: (_data, _error, { request, input }) => {
      wasQueued.delete(request);
      return options.onSettled
        ? options.onSettled(input)
        : queryClient.invalidateQueries({
            queryKey: moduleKeys[request.module],
          });
    },
  });

  const variables = (input: Input): QueuedVariables<Input> => {
    const request = toRequest(input);
    if (!isOnline()) wasQueued.add(request);
    return { request, input };
  };

  return {
    ...mutation,
    /** True while the write is pending and the network is there; a queued write offline is not «saving». */
    isSaving: mutation.isPending && !mutation.isPaused,
    mutate: (input: Input, callbacks?: Parameters<typeof mutation.mutate>[1]) =>
      mutation.mutate(variables(input), callbacks),
    mutateAsync: (
      input: Input,
      callbacks?: Parameters<typeof mutation.mutateAsync>[1],
    ) => mutation.mutateAsync(variables(input), callbacks),
  };
}
