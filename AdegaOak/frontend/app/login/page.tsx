'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import api from '@/lib/api';
import { useAuthStore } from '@/store/authStore';

export default function LoginPage() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const router = useRouter();
  const setAuth = useAuthStore((state) => state.setAuth);
  const user = useAuthStore((state) => state.user);
  const hasHydrated = useAuthStore((state) => state._hasHydrated);

  // If already logged in, redirect to dashboard
  useEffect(() => {
    if (hasHydrated && user) {
      router.replace('/dashboard');
    }
  }, [hasHydrated, user, router]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const { data } = await api.post('/auth/login', { username, password });
      setAuth(data.token, {
        username: data.username,
        nome: data.nome,
        role: data.role,
      });
      router.replace('/dashboard');
    } catch (err: any) {
      console.error('Login error:', err);
      
      // Extract error message from different possible error structures
      let errorMessage = 'Erro ao fazer login. Tente novamente.';
      
      if (err.response) {
        // Server responded with error
        if (err.response.status === 401) {
          errorMessage = 'Usuário ou senha incorretos.';
        } else if (err.response.data?.message) {
          errorMessage = err.response.data.message;
        } else if (err.response.statusText) {
          errorMessage = err.response.statusText;
        }
      } else if (err.message) {
        // Network or other error
        errorMessage = err.message;
      }
      
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  // Show spinner while rehydrating (avoids flash of login form for logged-in users)
  if (!hasHydrated) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-amber-50">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-amber-600" />
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 p-4 relative overflow-hidden">
      {/* Animated background elements */}
      <div className="absolute inset-0 overflow-hidden">
        <div className="absolute -top-40 -right-40 w-80 h-80 bg-amber-500/10 rounded-full blur-3xl animate-pulse" />
        <div className="absolute -bottom-40 -left-40 w-80 h-80 bg-orange-500/10 rounded-full blur-3xl animate-pulse delay-1000" />
      </div>

      <div className="relative bg-white/95 dark:bg-gray-900/95 backdrop-blur-xl p-10 rounded-2xl shadow-2xl w-full max-w-md border border-gray-200/50 dark:border-gray-700/50">
        <div className="text-center mb-10">
          <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-2 tracking-tight">
            Adega Oak
          </h1>
          <div className="h-1 w-20 bg-gradient-to-r from-amber-600 to-orange-600 mx-auto rounded-full mb-3" />
          <p className="text-gray-600 dark:text-gray-400 text-sm">
            Sistema de Gestão
          </p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="space-y-2">
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Usuário
            </label>
            <div className="relative group">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none transition-colors">
                <svg className="h-5 w-5 text-gray-400 group-focus-within:text-amber-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
              </div>
              <input
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                className="w-full pl-10 pr-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none transition-all bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400"
                placeholder="Digite seu usuário"
                autoComplete="username"
                required
              />
            </div>
          </div>

          <div className="space-y-2">
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Senha
            </label>
            <div className="relative group">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none transition-colors">
                <svg className="h-5 w-5 text-gray-400 group-focus-within:text-amber-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                </svg>
              </div>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full pl-10 pr-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-amber-500 focus:border-transparent outline-none transition-all bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400"
                placeholder="Digite sua senha"
                autoComplete="current-password"
                required
              />
            </div>
          </div>

          {error && (
            <div className="bg-red-50 dark:bg-red-900/30 border border-red-200 dark:border-red-800 text-red-700 dark:text-red-300 px-4 py-3 rounded-lg text-sm font-medium flex items-start gap-3 animate-shake">
              <svg className="h-5 w-5 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
              </svg>
              <span>{error}</span>
            </div>
          )}

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-gradient-to-r from-amber-600 to-orange-600 hover:from-amber-700 hover:to-orange-700 disabled:opacity-50 disabled:cursor-not-allowed text-white font-semibold py-3.5 rounded-lg transition-all transform hover:scale-[1.02] active:scale-[0.98] shadow-lg hover:shadow-xl"
          >
            {loading ? (
              <span className="flex items-center justify-center gap-2">
                <span className="animate-spin rounded-full h-5 w-5 border-b-2 border-white" />
                Entrando...
              </span>
            ) : (
              'Entrar'
            )}
          </button>
        </form>

        <div className="mt-8 text-center text-xs text-gray-500 dark:text-gray-400">
          <p>© 2024 Adega Oak. Todos os direitos reservados.</p>
        </div>
      </div>
    </div>
  );
}
