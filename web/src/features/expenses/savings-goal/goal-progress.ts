/** How far the jar is towards its goal. */
export interface GoalProgress {
  /** Whole percent done, never below 0; may pass 100. */
  percent: number;
  /** What is still missing, 0 once the goal is reached. */
  remaining: number;
  done: boolean;
}

/** Progress of `balance` towards a goal above zero. */
export function goalProgress(balance: number, goal: number): GoalProgress {
  const percent = Math.max(0, Math.floor((balance / goal) * 100));
  const remaining = Math.max(0, Math.round((goal - balance) * 100) / 100);
  return { percent, remaining, done: balance >= goal };
}
