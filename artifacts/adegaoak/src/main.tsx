import { createRoot } from "react-dom/client";
import App from "./App";
import "./index.css";

const stored = localStorage.getItem("adegaoak_theme") || "dark";
document.documentElement.classList.toggle("dark", stored === "dark");
document.documentElement.style.colorScheme = stored;

createRoot(document.getElementById("root")!).render(<App />);
