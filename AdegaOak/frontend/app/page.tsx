'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/store/authStore';

export default function Home() {
  const router = useRouter();
  const user = useAuthStore((state) => state.user);
  const hasHydrated = useAuthStore((state) => state._hasHydrated);

  useEffect(() => {
    // Wait until the store has rehydrated from localStorage before redirecting.
    if (!hasHydrated) return;

    if (user) {
      router.replace('/dashboard');
    } else {
      router.replace('/login');
    }
  }, [hasHydrated, user, router]);

  return (
    <div className="min-h-screen flex items-center justify-center bg-amber-50">
      <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-amber-600" />
    </div>
  );
}
