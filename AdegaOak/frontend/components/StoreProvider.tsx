'use client';

import { useEffect } from 'react';
import { useAuthStore } from '@/store/authStore';

/**
 * Triggers Zustand persist rehydration from localStorage after the client
 * mounts. This avoids the SSR/client hydration mismatch caused by reading
 * localStorage during server rendering.
 */
export default function StoreProvider({
  children,
}: {
  children: React.ReactNode;
}) {
  useEffect(() => {
    useAuthStore.persist.rehydrate();
    
    // Sync token to localStorage for axios interceptor after rehydration
    const token = useAuthStore.getState().token;
    if (token) {
      localStorage.setItem('token', token);
    }
  }, []);

  return <>{children}</>;
}
