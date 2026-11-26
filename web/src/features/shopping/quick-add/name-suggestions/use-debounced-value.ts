import { useEffect, useState } from 'react';

/** `value` once it has stopped changing for `delay` ms (the department preview, the keyword search). */
export function useDebouncedValue<T>(value: T, delay = 400): T {
  const [settled, setSettled] = useState(value);
  useEffect(() => {
    const timer = window.setTimeout(() => setSettled(value), delay);
    return () => window.clearTimeout(timer);
  }, [value, delay]);
  return settled;
}
