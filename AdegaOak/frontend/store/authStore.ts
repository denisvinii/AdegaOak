import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';

interface User {
  username: string;
  nome: string;
  role: string;
}

interface AuthState {
  token: string | null;
  user: User | null;
  _hasHydrated: boolean;
  setHasHydrated: (state: boolean) => void;
  setAuth: (token: string, user: User) => void;
  logout: () => void;
  isAdmin: () => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      token: null,
      user: null,
      _hasHydrated: false,

      setHasHydrated: (state) => set({ _hasHydrated: state }),

      setAuth: (token, user) => {
        // Mark as hydrated so guards don't redirect away immediately after login
        set({ token, user, _hasHydrated: true });
        // Also save token to localStorage for axios interceptor
        if (typeof window !== 'undefined') {
          localStorage.setItem('token', token);
        }
      },

      logout: () => {
        // Also clear hydration flag so guards don't flash content before redirect
        set({ token: null, user: null, _hasHydrated: true });
        // Remove token from localStorage
        if (typeof window !== 'undefined') {
          localStorage.removeItem('token');
        }
      },

      isAdmin: () => get().user?.role === 'admin',
    }),
    {
      name: 'auth-storage',
      storage: createJSONStorage(() => localStorage),
      // Don't rehydrate on the server — only on the client after mount.
      // This prevents the SSR/client mismatch that causes the hydration warning.
      skipHydration: true,
      onRehydrateStorage: () => (state) => {
        state?.setHasHydrated(true);
      },
    }
  )
);
