/**
 * Google Identity Services (web ADR 0015): the «Sign in with Google» button and One Tap. The script is loaded once,
 * only by the sign-in screen; tests put a fake `window.google` in place and never load the real one.
 */
export const GOOGLE_SCRIPT_URL = 'https://accounts.google.com/gsi/client';

/**
 * The button's language. Kept as the one literal here until the locale moves to `src/platform/locale.ts` (another
 * change adds it, with a rule against locale literals); reconcile at merge.
 */
const BUTTON_LOCALE = 'uk';

interface CredentialResponse {
  credential: string;
}

/** The part of `google.accounts.id` the app uses. */
export interface GoogleAccountsId {
  initialize(config: {
    client_id: string;
    callback: (response: CredentialResponse) => void;
    auto_select?: boolean;
    cancel_on_tap_outside?: boolean;
  }): void;
  renderButton(parent: HTMLElement, options: Record<string, unknown>): void;
  prompt(): void;
  disableAutoSelect(): void;
}

declare global {
  interface Window {
    google?: { accounts?: { id?: GoogleAccountsId } };
  }
}

let loading: Promise<GoogleAccountsId> | null = null;

const loaded = () => window.google?.accounts?.id;

/** Loads the Google script once; a failed load can be tried again. */
export function loadGoogleIdentity(): Promise<GoogleAccountsId> {
  const ready = loaded();
  if (ready) return Promise.resolve(ready);
  loading ??= new Promise<GoogleAccountsId>((resolve, reject) => {
    const script = document.createElement('script');
    script.src = GOOGLE_SCRIPT_URL;
    script.async = true;
    const fail = () => {
      loading = null;
      script.remove();
      reject(new Error('The Google sign-in script did not load.'));
    };
    script.onload = () => {
      const id = loaded();
      if (!id) return fail();
      loading = null; // from now on window.google answers
      resolve(id);
    };
    script.onerror = fail;
    document.head.append(script);
  });
  return loading;
}

export interface GoogleButtonOptions {
  clientId: string;
  /** The colour scheme the page shows now. */
  scheme: 'light' | 'dark';
  /** The button's width in pixels (Google allows 200–400). */
  width: number;
  /** False right after «Вийти»: no automatic sign-in until the member clicks the button again. */
  autoSelect: boolean;
  onCredential: (credential: string) => void;
}

/**
 * Draws the button into `host` and offers One Tap; `auto_select` signs a returning member in on a known device
 * without a click. After a sign-out Google is told to stop that (it remembers it until the next sign-in by click).
 */
export function showGoogleButton(
  id: GoogleAccountsId,
  host: HTMLElement,
  { clientId, scheme, width, autoSelect, onCredential }: GoogleButtonOptions,
) {
  if (!autoSelect) id.disableAutoSelect();
  id.initialize({
    client_id: clientId,
    callback: (response) => onCredential(response.credential),
    auto_select: autoSelect,
    cancel_on_tap_outside: true,
  });
  id.renderButton(host, {
    type: 'standard',
    theme: scheme === 'dark' ? 'filled_black' : 'outline',
    size: 'large',
    shape: 'pill',
    text: 'signin_with',
    logo_alignment: 'left',
    locale: BUTTON_LOCALE,
    width: Math.round(Math.min(400, Math.max(200, width))),
  });
  if (autoSelect) id.prompt();
}
