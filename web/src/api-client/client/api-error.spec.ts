import { describe, expect, it } from 'vitest';
import { ApiError } from './api-error';
import { apiCall } from './api-call';

describe('apiCall', () => {
  it('returns the data of a success', async () => {
    const data = await apiCall(
      Promise.resolve({
        data: { id: 1 },
        response: new Response(null, { status: 200 }),
      }),
    );

    expect(data).toEqual({ id: 1 });
  });

  it('throws an ApiError with the problem code, message and camelCase field errors', async () => {
    const problem = {
      detail: 'Name is required.',
      code: 'validation',
      errors: {
        CustomerName: ['Name is required.'],
        'Lines[0].Quantity': ['Too many.'],
      },
    };

    const failure = apiCall(
      Promise.resolve({
        error: problem,
        response: new Response(null, { status: 400 }),
      }),
    );

    await expect(failure).rejects.toMatchObject({
      status: 400,
      code: 'validation',
      message: 'Name is required.',
      fieldErrors: {
        customerName: ['Name is required.'],
        'lines[0].quantity': ['Too many.'],
      },
    });
    await expect(failure).rejects.toBeInstanceOf(ApiError);
  });

  it('falls back to the status when the body is not a problem', async () => {
    const failure = apiCall(
      Promise.resolve({
        error: undefined,
        response: new Response(null, { status: 502 }),
      }),
    );

    await expect(failure).rejects.toMatchObject({ code: 'http_502' });
  });
});
