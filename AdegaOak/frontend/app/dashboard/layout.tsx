'use client';

import { useEffect } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import Link from 'next/link';
import { useAuthStore } from '@/store/authStore';
import {
  LayoutDashboard,
  Package,
  TrendingUp,
  DollarSign,
  Wine,
  History,
  Users,
  LogOut,
  Menu,
  X,
  FileText,
} from 'lucide-react';
import { useState } from 'react';

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const router = useRouter();
  const pathname = usePathname();
  const { user, logout, isAdmin } = useAuthStore();
  const hasHydrated = useAuthStore((state) => state._hasHydrated);
  const [sidebarOpen, setSidebarOpen] = useState(false); // Closed by default on mobile
  const [isMobile, setIsMobile] = useState(false);

  useEffect(() => {
    if (!hasHydrated) return;
    if (!user) {
      router.replace('/login');
    }
  }, [hasHydrated, user, router]);

  // Detect mobile screen size
  useEffect(() => {
    const checkMobile = () => {
      setIsMobile(window.innerWidth < 768);
      if (window.innerWidth >= 768) {
        setSidebarOpen(true); // Open sidebar on desktop
      } else {
        setSidebarOpen(false); // Close sidebar on mobile
      }
    };

    checkMobile();
    window.addEventListener('resize', checkMobile);
    return () => window.removeEventListener('resize', checkMobile);
  }, []);

  // Close sidebar when navigating on mobile
  useEffect(() => {
    if (isMobile) {
      setSidebarOpen(false);
    }
  }, [pathname, isMobile]);

  // Show spinner while store is rehydrating or redirecting
  if (!hasHydrated || !user) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-amber-50 dark:bg-gray-900">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-amber-600" />
      </div>
    );
  }

  const handleLogout = () => {
    logout();
    router.push('/login');
  };

  const navItems = [
    { href: '/dashboard', icon: LayoutDashboard, label: 'Dashboard', adminOnly: true },
    { href: '/dashboard/estoque', icon: Package, label: 'Estoque' },
    { href: '/dashboard/movimentacoes', icon: TrendingUp, label: 'Movimentações' },
    { href: '/dashboard/despesas', icon: DollarSign, label: 'Despesas', adminOnly: true },
    { href: '/dashboard/combos', icon: Wine, label: 'Combos' },
    { href: '/dashboard/historico', icon: History, label: 'Histórico' },
    { href: '/dashboard/relatorios', icon: FileText, label: 'Relatórios', adminOnly: true },
    { href: '/dashboard/usuarios', icon: Users, label: 'Usuários', adminOnly: true },
  ];

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex">
      {/* Mobile Overlay */}
      {isMobile && sidebarOpen && (
        <div
          className="fixed inset-0 bg-black bg-opacity-50 z-40"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Sidebar */}
      <aside
        className={`${
          isMobile
            ? `fixed inset-y-0 left-0 z-50 w-64 transform transition-transform duration-300 ${
                sidebarOpen ? 'translate-x-0' : '-translate-x-full'
              }`
            : sidebarOpen
            ? 'w-64'
            : 'w-20'
        } bg-amber-900 dark:bg-gray-800 text-white flex flex-col border-r border-amber-800 dark:border-gray-700`}
      >
        <div className="p-4 md:p-6 flex items-center justify-between">
          {(sidebarOpen || isMobile) && (
            <h1 className="text-xl md:text-2xl font-bold">🍷 Adega Oak</h1>
          )}
          <button
            onClick={() => setSidebarOpen(!sidebarOpen)}
            className="p-2 hover:bg-amber-800 dark:hover:bg-gray-700 rounded transition-colors"
            aria-label={sidebarOpen ? 'Fechar menu' : 'Abrir menu'}
          >
            {isMobile && sidebarOpen ? <X size={24} /> : <Menu size={24} />}
          </button>
        </div>

        <nav className="flex-1 px-2 md:px-4 space-y-1 md:space-y-2 overflow-y-auto">
          {navItems.map((item) => {
            if (item.adminOnly && !isAdmin()) return null;
            const isActive = pathname === item.href;
            return (
              <Link
                key={item.href}
                href={item.href}
                className={`flex items-center gap-3 px-3 md:px-4 py-3 rounded-lg transition-colors ${
                  isActive
                    ? 'bg-amber-800 dark:bg-gray-700'
                    : 'hover:bg-amber-800 dark:hover:bg-gray-700'
                }`}
              >
                <item.icon size={20} className="flex-shrink-0" />
                {(sidebarOpen || isMobile) && <span className="text-sm md:text-base">{item.label}</span>}
              </Link>
            );
          })}
        </nav>

        <div className="p-3 md:p-4 border-t border-amber-800 dark:border-gray-700">
          <div className="flex items-center gap-3 px-2 md:px-4 py-2">
            {(sidebarOpen || isMobile) && (
              <div className="flex-1 min-w-0">
                <p className="font-semibold text-sm md:text-base truncate">{user.nome}</p>
                <p className="text-xs text-amber-300 dark:text-gray-400 truncate">
                  {user.role === 'admin' ? 'Administrador' : 'Funcionário'}
                </p>
              </div>
            )}
            <button
              onClick={handleLogout}
              className="p-2 hover:bg-amber-800 dark:hover:bg-gray-700 rounded transition-colors flex-shrink-0"
              title="Sair"
              aria-label="Sair"
            >
              <LogOut size={20} />
            </button>
          </div>
        </div>
      </aside>

      {/* Main Content */}
      <main className="flex-1 overflow-auto">
        {/* Mobile Header */}
        {isMobile && (
          <div className="sticky top-0 z-30 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 px-4 py-3 flex items-center justify-between md:hidden">
            <button
              onClick={() => setSidebarOpen(true)}
              className="p-2 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg transition-colors"
              aria-label="Abrir menu"
            >
              <Menu size={24} className="text-gray-700 dark:text-gray-300" />
            </button>
            <h2 className="text-lg font-bold text-gray-900 dark:text-white">Adega Oak</h2>
            <div className="w-10" /> {/* Spacer for centering */}
          </div>
        )}

        <div className="p-4 md:p-6 lg:p-8">{children}</div>
      </main>
    </div>
  );
}
