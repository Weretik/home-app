/** How far the jar is towards its goal. */
export interface GoalProgress {
  /** Whole percent done, never below 0; may pass 100. */
  percent: number;
  /** What is still missing, 0 once the goal is reached. */
  remaining: number;
  done: boolean;
}

/**
 * How much of the savings goal the jar holds, in whole percent rounded down (100 only once the goal is reached), never
 * below 0, 0 without a goal. The home summary's «Копилка» card uses the same rule (`goalPercentOf` in
 * `home/expenses-summary/data/money.ts`); features never import each other, so both sides test the same cases.
 */
export const goalPercent = (balance: number, goal: number) =>
  goal > 0 ? Math.max(0, Math.floor((balance / goal) * 100)) : 0;

/** Progress of `balance` towards a goal above zero. */
export function goalProgress(balance: number, goal: number): GoalProgress {
  const percent = goalPercent(balance, goal);
  const remaining = Math.max(0, Math.round((goal - balance) * 100) / 100);
  return { percent, remaining, done: balance >= goal };
}
