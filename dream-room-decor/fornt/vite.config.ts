// @lovable.dev/vite-tanstack-config already includes the following — do NOT add them manually
// or the app will break with duplicate plugins:
//   - tanstackStart, viteReact, tailwindcss, tsConfigPaths, cloudflare (build-only),
//     componentTagger (dev-only), VITE_* env injection, @ path alias, React/TanStack dedupe,
//     error logger plugins, and sandbox detection (port/host/strictPort).
// You can pass additional config via defineConfig({ vite: { ... } }) if needed.
import { defineConfig } from "@lovable.dev/vite-tanstack-config";

// The backend (ASP.NET Core) has no CORS configured and must not be edited.
// We proxy /api through the dev server so the browser stays same-origin and
// never triggers a CORS preflight. Point this at the backend's HTTP endpoint.
const API_TARGET = process.env.VITE_API_TARGET ?? "http://localhost:5039";

export default defineConfig({
  vite: {
    server: {
      proxy: {
        "/api": {
          target: API_TARGET,
          changeOrigin: true,
          secure: false,
        },
        // The AI Room controller is routed at /AiRoom (not under /api),
        // so it needs its own proxy entry to stay same-origin.
        "/AiRoom": {
          target: API_TARGET,
          changeOrigin: true,
          secure: false,
        },
      },
    },
  },
});
