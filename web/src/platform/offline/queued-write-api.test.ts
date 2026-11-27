import {
  MutationCache,
  onlineManager,
  QueryClient,
} from '@tanstack/react-query';
import { afterEach, vi } from 'vitest';
import { ApiError } from '../../api-client';
import { refusedWrites } from './network-status';
import type { QueuedRequest } from './queued-request';
import {
  registerQueuedWrites,
  retryQueuedWrite,
  sendQueuedRequest,
} from './queued-write-api';

interface Sent {
  method: string;
  path: string;
  headers: Headers;
  body: unknown;
}

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  });

function stubApi(...answers: Array<Response | Error>) {
  const sent: Sent[] = [];
  vi.stubGlobal('fetch', async (request: Request) => {
    const text = await request.text();
    sent.push({
      method: request.method,
      path: new URL(request.url).pathname,
      headers: request.headers,
      body: text ? JSON.parse(text) : undefined,
    });
    const answer = answers.shift() ?? json({});
    if (answer instanceof Error) throw answer;
    return answer;
  });
  return sent;
}

const move: QueuedRequest = {
  method: 'put',
  path: '/api/todos/tasks/t1/status',
  body: { status: 'Done' },
  headers: { 'If-Match': '3' },
  module: 'todos',
  label: 'позначку «зроблено»',
  refetch: '/api/todos/tasks/t1',
};

afterEach(() => {
  vi.unstubAllGlobals();
  refusedWrites.clear();
  onlineManager.setOnline(true);
});

test('sends the request with its body and headers and returns the answer', async () => {
  const sent = stubApi(json({ id: 't1', version: 4 }));

  await expect(sendQueuedRequest(move)).resolves.toEqual({
    id: 't1',
    version: 4,
  });

  expect(sent).toHaveLength(1);
  expect(sent[0]).toMatchObject({
    method: 'PUT',
    path: '/api/todos/tasks/t1/status',
    body: { status: 'Done' },
  });
  expect(sent[0]!.headers.get('If-Match')).toBe('3');
});

test('keeps the Idempotency-Key of a create', async () => {
  const sent = stubApi(json({ added: [] }, 201));

  await sendQueuedRequest({
    method: 'post',
    path: '/api/shopping/items',
    body: { text: 'молоко' },
    headers: { 'Idempotency-Key': 'key-1' },
    module: 'shopping',
    label: 'товари «молоко»',
  });

  expect(sent[0]!.headers.get('Idempotency-Key')).toBe('key-1');
});

test.each([404, 410])(
  'a record gone meanwhile (%i) counts as done and nothing is recreated',
  async (status) => {
    const sent = stubApi(json({ title: 'Gone' }, status));

    await expect(sendQueuedRequest(move)).resolves.toBeUndefined();
    expect(sent).toHaveLength(1);
  },
);

test.each([409, 412])(
  'a stale write (%i) reads the record and is sent once more with its version (last write wins)',
  async (status) => {
    const sent = stubApi(
      json({ code: 'todos.changed_meanwhile' }, status),
      json({ id: 't1', version: 9 }),
      json({ id: 't1', version: 10 }),
    );

    await expect(sendQueuedRequest(move)).resolves.toEqual({
      id: 't1',
      version: 10,
    });

    expect(sent.map((s) => `${s.method} ${s.path}`)).toEqual([
      'PUT /api/todos/tasks/t1/status',
      'GET /api/todos/tasks/t1',
      'PUT /api/todos/tasks/t1/status',
    ]);
    expect(sent[2]!.headers.get('If-Match')).toBe('9');
  },
);

test('a record deleted before the reread stays deleted', async () => {
  stubApi(json({}, 409), json({ title: 'Not found' }, 404));

  await expect(sendQueuedRequest(move)).resolves.toBeUndefined();
});

test('a write stale twice is refused with the second answer', async () => {
  stubApi(
    json({}, 409),
    json({ version: 9 }),
    json({ code: 'todos.changed_meanwhile' }, 409),
  );

  await expect(sendQueuedRequest(move)).rejects.toMatchObject({
    status: 409,
    code: 'todos.changed_meanwhile',
  });
});

test('a stale write without a refetch path, or a reread without a version, is refused', async () => {
  stubApi(json({ code: 'conflict' }, 409));
  await expect(
    sendQueuedRequest({ ...move, refetch: undefined }),
  ).rejects.toMatchObject({ status: 409, code: 'conflict' });

  stubApi(json({ code: 'conflict' }, 409), json({ id: 't1' }));
  await expect(sendQueuedRequest(move)).rejects.toMatchObject({
    code: 'conflict',
  });
});

test('another refusal throws the ApiError; a network failure throws as it is', async () => {
  stubApi(json({ code: 'validation' }, 400));
  await expect(sendQueuedRequest(move)).rejects.toBeInstanceOf(ApiError);

  stubApi(new TypeError('Failed to fetch'));
  await expect(sendQueuedRequest(move)).rejects.toBeInstanceOf(TypeError);
});

test('retries a network failure always, a server error three times, a refusal never', () => {
  expect(retryQueuedWrite(10, new TypeError('Failed to fetch'))).toBe(true);
  expect(retryQueuedWrite(2, new ApiError(503, {}))).toBe(true);
  expect(retryQueuedWrite(3, new ApiError(503, {}))).toBe(false);
  expect(retryQueuedWrite(0, new ApiError(400, {}))).toBe(false);
});

describe('the defaults of a restored queued write', () => {
  const client = () => {
    const queryClient = new QueryClient({ mutationCache: new MutationCache() });
    registerQueuedWrites(queryClient);
    return queryClient;
  };
  const run = (queryClient: QueryClient, request: QueuedRequest) =>
    queryClient
      .getMutationCache()
      .build(queryClient, { mutationKey: ['queued-write'] })
      .execute({ request, input: undefined });

  test('sends the request and refreshes its module', async () => {
    const sent = stubApi(json({ version: 4 }));
    const queryClient = client();
    const invalidate = vi.spyOn(queryClient, 'invalidateQueries');

    await run(queryClient, move);

    expect(sent).toHaveLength(1);
    expect(invalidate).toHaveBeenCalledWith({ queryKey: ['todos'] });
  });

  test('records a refusal for the snackbar', async () => {
    stubApi(json({ code: 'validation' }, 400));

    await expect(run(client(), move)).rejects.toBeInstanceOf(ApiError);

    expect(refusedWrites.current().map((w) => w.label)).toEqual([
      'позначку «зроблено»',
    ]);
  });
});
