import { useEffect, useRef } from "react";
import { endSessionDueToUnauthorized } from "./endSession";

const ACTIVITY_EVENTS: (keyof DocumentEventMap)[] = [
  "mousemove",
  "mousedown",
  "keydown",
  "scroll",
  "touchstart",
];

const DEFAULT_TIMEOUT_MS = 30 * 60 * 1000; // 30 minutes

/**
 * Auto-logout the user after a period of inactivity.
 * Only active when `enabled` is true (i.e. user is signed in).
 */
export function useInactivityLogout(
  enabled: boolean,
  onLogout: () => void,
  timeoutMs = DEFAULT_TIMEOUT_MS
) {
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  useEffect(() => {
    if (!enabled) return;

    function resetTimer() {
      if (timerRef.current) clearTimeout(timerRef.current);
      timerRef.current = setTimeout(() => {
        endSessionDueToUnauthorized();
        onLogout();
      }, timeoutMs);
    }

    resetTimer();

    for (const evt of ACTIVITY_EVENTS) {
      document.addEventListener(evt, resetTimer, { passive: true });
    }

    return () => {
      if (timerRef.current) clearTimeout(timerRef.current);
      for (const evt of ACTIVITY_EVENTS) {
        document.removeEventListener(evt, resetTimer);
      }
    };
  }, [enabled, onLogout, timeoutMs]);
}
