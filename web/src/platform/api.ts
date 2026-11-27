import { createApiClient } from '../api-client';
import { sessionMiddleware } from './session-middleware';

/**
 * The typed API client. The API lives on another origin (Azure App Service), so VITE_API_URL is set at build time;
 * locally it points at http://localhost:5000 (see .env.example). Every call carries the session token.
 */
export const api = createApiClient({
  baseUrl: (import.meta.env.VITE_API_URL ?? '').replace(/\/$/, ''),
});

api.use(sessionMiddleware);
