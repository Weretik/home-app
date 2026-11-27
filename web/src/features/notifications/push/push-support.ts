/** What this browser can do about push (notifications 002, FR-103). */
export type PushSupport = 'supported' | 'ios-needs-install' | 'unsupported';

export interface BrowserTraits {
  userAgent: string;
  /** Started from the home screen (`display-mode: standalone`, or Safari's `navigator.standalone`). */
  standalone: boolean;
  hasServiceWorker: boolean;
  hasPushManager: boolean;
  hasNotification: boolean;
}

const isIos = (userAgent: string) =>
  /iPhone|iPad|iPod/.test(userAgent) ||
  // iPadOS asks for the desktop site: a Mac with touch.
  (/Macintosh/.test(userAgent) && /Mobile\//.test(userAgent));

/** An iPhone or iPad offers push only to the app started from the home screen (iOS 16.4+). */
export function pushSupport(traits: BrowserTraits): PushSupport {
  if (
    traits.hasServiceWorker &&
    traits.hasPushManager &&
    traits.hasNotification
  )
    return 'supported';
  return isIos(traits.userAgent) && !traits.standalone
    ? 'ios-needs-install'
    : 'unsupported';
}

/** A short device label from the user agent: «iPhone · Safari», «Android · Chrome», «Windows · Edge». */
export function deviceLabel(userAgent: string): string {
  const system = /iPhone/.test(userAgent)
    ? 'iPhone'
    : /iPad/.test(userAgent)
      ? 'iPad'
      : /Android/.test(userAgent)
        ? 'Android'
        : /Windows/.test(userAgent)
          ? 'Windows'
          : /Mac OS X|Macintosh/.test(userAgent)
            ? 'Mac'
            : /Linux/.test(userAgent)
              ? 'Linux'
              : 'Пристрій';
  const browser = /Edg\//.test(userAgent)
    ? 'Edge'
    : /Firefox\/|FxiOS/.test(userAgent)
      ? 'Firefox'
      : /SamsungBrowser/.test(userAgent)
        ? 'Samsung Internet'
        : /Chrome\/|CriOS/.test(userAgent)
          ? 'Chrome'
          : /Safari\//.test(userAgent)
            ? 'Safari'
            : 'браузер';
  return `${system} · ${browser}`;
}

/** The VAPID public key (base64url) as the bytes `pushManager.subscribe` needs. */
export function decodeKey(base64Url: string): Uint8Array<ArrayBuffer> {
  const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
  const padded = base64 + '='.repeat((4 - (base64.length % 4)) % 4);
  const binary = atob(padded);
  const bytes = new Uint8Array(new ArrayBuffer(binary.length));
  for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
  return bytes;
}

/** A browser subscription as the API stores it. */
export function subscriptionBody(
  subscription: PushSubscriptionJSON,
  device: string,
) {
  return {
    endpoint: subscription.endpoint ?? '',
    p256dh: subscription.keys?.p256dh ?? '',
    auth: subscription.keys?.auth ?? '',
    device,
  };
}

/** Why the switch cannot be offered on this device, or null (FR-102, FR-103). */
export function pushBlocker(
  status: { configured: boolean; canSubscribe: boolean },
  support: PushSupport,
): string | null {
  if (!status.configured)
    return 'Push-сповіщення ще не налаштовано: додайте WebPush__PublicKey і WebPush__PrivateKey у налаштування App Service.';
  if (!status.canSubscribe)
    return 'Push-сповіщення доступні лише Віталію і Маші.';
  if (support === 'ios-needs-install')
    return 'На iPhone чи iPad додайте «Дім» на екран «Додому» (Поділитися → «На екран Додому») і відкрийте звідти.';
  if (support === 'unsupported')
    return 'Цей браузер не підтримує push-сповіщення.';
  return null;
}
