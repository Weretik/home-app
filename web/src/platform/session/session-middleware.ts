import type { ApiClient } from '../../api-client';
import { session } from './session';

type Middleware = Parameters<ApiClient['use']>[0];

/**
 * Every call carries the session token (web ADR 0015, FR-005). A 401 to a call that carried the stored token means
 * the session is over: the token is forgotten and the sign-in screen says so. A 401 to an older token is ignored, so
 * a late answer never ends a newer session; a 401 without a token (a refused sign-in) changes nothing.
 */
export const sessionMiddleware: Middleware = {
  onRequest({ request }) {
    const token = session.token();
    if (token) request.headers.set('Authorization', `Bearer ${token}`);
    return request;
  },
  onResponse({ request, response }) {
    const sent = request.headers.get('Authorization');
    const token = session.token();
    if (response.status === 401 && token && sent === `Bearer ${token}`)
      session.clear('expired');
    return response;
  },
};
