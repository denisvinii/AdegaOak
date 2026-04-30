'use client';

import { Loader2 } from 'lucide-react';

interface LoadingButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  loading?: boolean;
  loadingText?: string;
  children: React.ReactNode;
}

export default function LoadingButton({
  loading = false,
  loadingText,
  children,
  disabled,
  className = '',
  ...props
}: LoadingButtonProps) {
  return (
    <button
      {...props}
      disabled={disabled || loading}
      className={`flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed transition ${className}`}
    >
      {loading && <Loader2 size={16} className="animate-spin flex-shrink-0" />}
      {loading && loadingText ? loadingText : children}
    </button>
  );
}
