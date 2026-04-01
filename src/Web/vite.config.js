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
      '/collaboration': {
        target: 'http://collaboration.api:8080',
        changeOrigin: true,
        ws: true
      }
    }
  }
})
