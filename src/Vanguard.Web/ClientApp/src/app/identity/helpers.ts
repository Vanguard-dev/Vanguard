import { Session } from './session';

const VANGUARD_SESSION_KEY = 'VANGUARD_SESSION';

let _session;
export const setSession = (session: Session) => _session = session;
export const getSession = () => _session;

export const loadSession = () => {
  const storedValue = localStorage.getItem(VANGUARD_SESSION_KEY);
  return !storedValue ? {} : JSON.parse(storedValue);
};

export const saveSession = (session: Session) => {
  localStorage.setItem(VANGUARD_SESSION_KEY, JSON.stringify(session));
};
