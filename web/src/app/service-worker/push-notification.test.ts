import {
  clickTarget,
  GENERIC_MESSAGE,
  notificationOptions,
  parsePushPayload,
  sameOriginClient,
} from './push-notification';

const origin = 'https://home.example';

test('reads the API’s push payload', () => {
  expect(
    parsePushPayload(
      JSON.stringify({
        title: 'Справи на сьогодні',
        body: '3 справи',
        url: '/todos?when=today',
        tag: 'todos-morning',
      }),
    ),
  ).toEqual({
    title: 'Справи на сьогодні',
    body: '3 справи',
    url: '/todos?when=today',
    tag: 'todos-morning',
  });
});

test('fills a missing body, url and tag', () => {
  expect(parsePushPayload('{"title":"Покупки"}')).toEqual({
    title: 'Покупки',
    body: '',
    url: '/',
    tag: undefined,
  });
});

test.each([
  ['nothing', undefined],
  ['empty', ''],
  ['not JSON', 'hello'],
  ['not an object', '42'],
  ['null', 'null'],
  ['no title', '{"body":"x"}'],
  ['a blank title', '{"title":"  "}'],
])(
  'a malformed payload (%s) shows the generic «Дім» notification',
  (_, payload) => {
    expect(parsePushPayload(payload)).toEqual(GENERIC_MESSAGE);
    expect(GENERIC_MESSAGE.title).toBe('Дім');
  },
);

test('the notification has the app icon, its tag and its page', () => {
  expect(
    notificationOptions({ title: 'T', body: 'B', url: '/gifts', tag: 'g1' }),
  ).toEqual({
    body: 'B',
    icon: '/icon-192.png',
    badge: '/icon-192.png',
    tag: 'g1',
    data: { url: '/gifts' },
  });
});

test.each([
  ['/todos', `${origin}/todos`],
  [`${origin}/shopping?x=1`, `${origin}/shopping?x=1`],
  ['https://evil.example/phish', `${origin}/`],
  ['javascript:alert(1)', `${origin}/`],
  [42, `${origin}/`],
  [undefined, `${origin}/`],
])('a tap on %s opens %s (same origin only)', (url, expected) => {
  expect(clickTarget(url, origin)).toBe(expected);
});

test('reuses an open window of this site only', () => {
  const windows = [
    { url: 'https://other.example/' },
    { url: `${origin}/expenses` },
  ];

  expect(sameOriginClient(windows, origin)).toBe(windows[1]);
  expect(sameOriginClient([{ url: 'about:blank' }], origin)).toBeUndefined();
});
