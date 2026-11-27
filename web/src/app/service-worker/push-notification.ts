/**
 * The push messages of the API (api notifications/002): `{ title, body, url, tag }` as JSON. Pure functions, so the
 * service worker stays a thin shell and this is tested without a worker.
 */
export interface PushMessage {
  title: string;
  body: string;
  /** The page to open, a path of this site such as `/todos`. */
  url: string;
  tag?: string;
}

/** Shown when the payload is missing or not the expected JSON. */
export const GENERIC_MESSAGE: PushMessage = {
  title: 'Дім',
  body: 'Нове сповіщення',
  url: '/',
};

export const NOTIFICATION_ICON = '/icon-192.png';

const text = (value: unknown) =>
  typeof value === 'string' && value.trim() !== '' ? value : undefined;

/** Reads a push payload; anything malformed becomes the generic «Дім» notification. */
export function parsePushPayload(
  payload: string | null | undefined,
): PushMessage {
  if (!payload) return GENERIC_MESSAGE;
  let data: unknown;
  try {
    data = JSON.parse(payload);
  } catch {
    return GENERIC_MESSAGE;
  }
  if (typeof data !== 'object' || data === null) return GENERIC_MESSAGE;
  const fields = data as Record<string, unknown>;
  const title = text(fields.title);
  if (!title) return GENERIC_MESSAGE;
  return {
    title,
    body: text(fields.body) ?? '',
    url: text(fields.url) ?? '/',
    tag: text(fields.tag),
  };
}

/** The options of `showNotification` for a message: the app icon, its page in `data.url`, its tag. */
export function notificationOptions(message: PushMessage) {
  return {
    body: message.body,
    icon: NOTIFICATION_ICON,
    badge: NOTIFICATION_ICON,
    tag: message.tag,
    data: { url: message.url },
  };
}

/** The absolute URL a tap opens: the message's page when it is on this site, else the home page. */
export function clickTarget(url: unknown, origin: string): string {
  const home = new URL('/', origin).href;
  if (typeof url !== 'string') return home;
  try {
    const target = new URL(url, origin);
    return target.origin === new URL(origin).origin ? target.href : home;
  } catch {
    return home;
  }
}

/** The open window of this site to reuse for a tap, if any. */
export function sameOriginClient<Client extends { url: string }>(
  clients: readonly Client[],
  origin: string,
): Client | undefined {
  const own = new URL(origin).origin;
  return clients.find((client) => {
    try {
      return new URL(client.url).origin === own;
    } catch {
      return false;
    }
  });
}
