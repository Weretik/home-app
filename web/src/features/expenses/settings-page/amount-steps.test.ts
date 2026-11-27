import { formatAmount } from '../data/formatting';
import {
  earlierAmountCaption,
  formatSince,
  monthlyLimitHint,
  nextAmountCaption,
  nextStep,
  stepsAround,
} from './amount-steps';

const steps = [
  { from: '2000-01-01', amount: 7000 },
  { from: '2026-12-01', amount: 5000 },
  { from: '2027-04-01', amount: 6000 },
];

test('picks the step in force in a month and the one before it', () => {
  expect(stepsAround(steps, '2026-12')).toEqual({
    current: steps[1],
    before: steps[0],
  });
  expect(stepsAround(steps, '2027-03')).toEqual({
    current: steps[1],
    before: steps[0],
  });
  expect(stepsAround(steps, '2027-05')).toEqual({
    current: steps[2],
    before: steps[1],
  });
});

test('nothing before the first step, a single step or no steps', () => {
  expect(stepsAround(steps, '2026-11')).toBeUndefined();
  expect(stepsAround([steps[0]!], '2026-12')).toBeUndefined();
  expect(stepsAround([], '2026-12')).toBeUndefined();
});

test('names the month a step starts in, in the genitive', () => {
  expect(formatSince('2026-12-01')).toBe('з грудня 2026');
  expect(formatSince('2027-01-01')).toBe('з січня 2027');
  expect(formatSince('2026-10-01')).toBe('з жовтня 2026');
});

test('the caption takes today in Kyiv', () => {
  expect(
    earlierAmountCaption(steps.slice(0, 2), new Date('2026-11-30T22:30:00Z')),
  ).toBe(`з грудня 2026 · раніше ${formatAmount(7000)}`);
  expect(
    earlierAmountCaption(steps.slice(0, 2), new Date('2026-11-30T21:30:00Z')),
  ).toBeUndefined();
  expect(
    earlierAmountCaption([steps[0]!], new Date('2026-12-10T10:00:00Z')),
  ).toBeUndefined();
});

test('the next step is the first one after the month', () => {
  expect(nextStep(steps, '2026-11')).toEqual(steps[1]);
  expect(nextStep(steps, '2026-12')).toEqual(steps[2]);
  expect(nextStep(steps, '2027-04')).toBeUndefined();
  expect(nextStep([], '2026-12')).toBeUndefined();
});

test('a change saved ahead is captioned with its month and amount', () => {
  const ahead = [
    { from: '2000-01-01', amount: 7000 },
    { from: '2027-04-01', amount: 6000 },
  ];
  const december = new Date('2026-12-10T10:00:00Z');
  expect(nextAmountCaption(ahead, december)).toBe(
    `з квітня 2027 — ${formatAmount(6000)}`,
  );
  expect(earlierAmountCaption(ahead, december)).toBeUndefined();
  expect(
    nextAmountCaption(ahead, new Date('2027-04-01T10:00:00Z')),
  ).toBeUndefined();
});

test('the monthly limit hint joins the earlier and the next amount', () => {
  const december = new Date('2026-12-10T10:00:00Z');
  expect(monthlyLimitHint([steps[0]!], december)).toBe('Порожньо — без ліміту');
  expect(monthlyLimitHint(steps.slice(0, 2), december)).toBe(
    `з грудня 2026 · раніше ${formatAmount(7000)}; порожньо — без ліміту`,
  );
  expect(monthlyLimitHint(steps, december)).toBe(
    `з грудня 2026 · раніше ${formatAmount(7000)}; з квітня 2027 — ${formatAmount(6000)}; порожньо — без ліміту`,
  );
});
