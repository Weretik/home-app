import { describe, expect, it } from 'vitest';
import {
  decodeKey,
  deviceLabel,
  pushBlocker,
  pushSupport,
  subscriptionBody,
  type BrowserTraits,
} from './push-support';

const iphone =
  'Mozilla/5.0 (iPhone; CPU iPhone OS 17_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4 Mobile/15E148 Safari/604.1';
const android =
  'Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0 Mobile Safari/537.36';
const edge =
  'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0 Safari/537.36 Edg/131.0';

const traits = (over: Partial<BrowserTraits>): BrowserTraits => ({
  userAgent: android,
  standalone: false,
  hasServiceWorker: true,
  hasPushManager: true,
  hasNotification: true,
  ...over,
});

describe('pushSupport', () => {
  it('is supported where the browser has push', () => {
    expect(pushSupport(traits({}))).toBe('supported');
  });

  it('asks an iPhone outside the installed app to add it to the home screen', () => {
    expect(
      pushSupport(
        traits({
          userAgent: iphone,
          hasPushManager: false,
          hasNotification: false,
        }),
      ),
    ).toBe('ios-needs-install');
  });

  it('says unsupported elsewhere', () => {
    expect(pushSupport(traits({ hasPushManager: false }))).toBe('unsupported');
  });
});

describe('deviceLabel', () => {
  it('names the system and the browser', () => {
    expect(deviceLabel(iphone)).toBe('iPhone · Safari');
    expect(deviceLabel(android)).toBe('Android · Chrome');
    expect(deviceLabel(edge)).toBe('Windows · Edge');
  });
});

describe('decodeKey and subscriptionBody', () => {
  it('decodes base64url without padding', () => {
    expect([...decodeKey('AP8-_w')]).toEqual([0, 255, 62, 255]);
  });

  it('takes the endpoint and keys of a subscription', () => {
    expect(
      subscriptionBody(
        {
          endpoint: 'https://push.example/1',
          keys: { p256dh: 'p', auth: 'a' },
        },
        'Mac · Safari',
      ),
    ).toEqual({
      endpoint: 'https://push.example/1',
      p256dh: 'p',
      auth: 'a',
      device: 'Mac · Safari',
    });
  });
});

describe('pushBlocker', () => {
  it('names what stops push, or nothing', () => {
    const ready = { configured: true, canSubscribe: true };
    expect(pushBlocker({ ...ready, configured: false }, 'supported')).toMatch(
      /не налаштовано/,
    );
    expect(pushBlocker({ ...ready, canSubscribe: false }, 'supported')).toMatch(
      /лише Віталію і Маші/,
    );
    expect(pushBlocker(ready, 'ios-needs-install')).toMatch(/На екран Додому/);
    expect(pushBlocker(ready, 'unsupported')).toMatch(/не підтримує/);
    expect(pushBlocker(ready, 'supported')).toBeNull();
  });
});
