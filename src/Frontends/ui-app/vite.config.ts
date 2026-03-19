import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      // Same-origin /api in dev → UserService (no browser CORS)
      "/api": {
        target: process.env.VITE_DEV_API_PROXY ?? "http://localhost:5001",
        changeOrigin: true,
      },
    },
  },
});
