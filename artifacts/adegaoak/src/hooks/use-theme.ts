import { useEffect, useState } from "react";

const KEY = "adegaoak_theme";
export type Theme = "light" | "dark";

function apply(t: Theme) {
  const el = document.documentElement;
  el.classList.toggle("dark", t === "dark");
  el.style.colorScheme = t;
}

export function useTheme() {
  const [theme, setTheme] = useState<Theme>(() => {
    if (typeof window === "undefined") return "dark";
    return (localStorage.getItem(KEY) as Theme) || "dark";
  });

  useEffect(() => {
    apply(theme);
    localStorage.setItem(KEY, theme);
  }, [theme]);

  return { theme, setTheme, toggle: () => setTheme((t) => (t === "dark" ? "light" : "dark")) };
}
