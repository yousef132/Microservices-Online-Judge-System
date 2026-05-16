import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    host: true,
    port: 5173,
    watch: {
      usePolling: true,
      interval: 100,
    },
    proxy: {
      '/community-api': {
        target: 'https://localhost:7014',
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.replace(/^\/community-api/, '')
      },
      '/collaboration': {
        target: 'http://collaboration.api:8080',
        changeOrigin: true,
        ws: true
      }
    }
  }
})
