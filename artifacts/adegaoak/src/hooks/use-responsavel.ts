import { useState, useEffect } from "react";

const STORAGE_KEY = "adegaoak_responsavel";

export function useResponsavel() {
  const [responsavel, setResponsavel] = useState<string>(() => {
    return localStorage.getItem(STORAGE_KEY) || "";
  });

  useEffect(() => {
    if (responsavel) {
      localStorage.setItem(STORAGE_KEY, responsavel);
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
  }, [responsavel]);

  return { responsavel, setResponsavel };
}
