import type { Metadata } from "next";
import "./globals.css";
import StoreProvider from "@/components/StoreProvider";

export const metadata: Metadata = {
  title: "Adega Oak",
  description: "Sistema de Gestão - Adega Oak",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    // suppressHydrationWarning prevents browser extensions (e.g. crypto wallets)
    // from causing hydration mismatches by injecting attributes into <html>/<body>.
    <html lang="pt-BR" suppressHydrationWarning className="dark">
      <body className="min-h-screen bg-gray-50 dark:bg-gray-900" suppressHydrationWarning>
        <StoreProvider>{children}</StoreProvider>
      </body>
    </html>
  );
}
