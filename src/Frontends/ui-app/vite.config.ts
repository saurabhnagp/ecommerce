import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/product-api": {
        target: process.env.VITE_DEV_PRODUCT_API_PROXY ?? "http://localhost:5002",
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/product-api/, "/api"),
      },
      "/api": {
        target: process.env.VITE_DEV_API_PROXY ?? "http://localhost:5001",
        changeOrigin: true,
      },
    },
  },
});
