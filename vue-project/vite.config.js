import { fileURLToPath, URL } from 'node:url'

import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const devUrl = env.VITE_DEVURL

  const proxyConfig = {}
  if (devUrl) {
    proxyConfig['/crm-api'] = {
      target: devUrl,
      changeOrigin: true,
      secure: true,
      rewrite: (path) => path.replace(/^\/crm-api/, ''),
      cookieDomainRewrite: 'localhost'
    }
  }

  return {
    plugins: [vue()],
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url))
      }
    },
    server: {
      proxy: proxyConfig
    }
  }
})