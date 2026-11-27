import { useEffect, useRef, useState } from 'react';
import { loadGoogleIdentity, showGoogleButton } from '../data/google-identity';

export type GoogleButtonStatus = 'loading' | 'ready' | 'failed';

interface Options {
  clientId: string;
  scheme: 'light' | 'dark';
  autoSelect: boolean;
  onCredential: (credential: string) => void;
}

/**
 * Loads Google's script and draws its button into `host` (FR-001, FR-004); draws it again when the colour scheme
 * changes. `retry` loads the script again after a failure.
 */
export function useGoogleButton({
  clientId,
  scheme,
  autoSelect,
  onCredential,
}: Options) {
  const host = useRef<HTMLDivElement>(null);
  const latest = useRef(onCredential);
  const [status, setStatus] = useState<GoogleButtonStatus>('loading');
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    latest.current = onCredential;
  }, [onCredential]);

  useEffect(() => {
    let active = true;
    loadGoogleIdentity().then(
      (id) => {
        const element = host.current;
        if (!active || !element) return;
        element.replaceChildren();
        showGoogleButton(id, element, {
          clientId,
          scheme,
          autoSelect,
          width: element.parentElement?.clientWidth ?? 0,
          onCredential: (credential) => latest.current(credential),
        });
        setStatus('ready');
      },
      () => {
        if (active) setStatus('failed');
      },
    );
    return () => {
      active = false;
    };
  }, [clientId, scheme, autoSelect, attempt]);

  const retry = () => {
    setStatus('loading');
    setAttempt((count) => count + 1);
  };

  return { host, status, retry };
}
