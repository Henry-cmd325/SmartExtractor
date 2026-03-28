// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  modules: [
    '@nuxt/eslint',
    '@nuxt/ui'
  ],

  runtimeConfig: {
    public: {
      // El valor aquí es el "default" por si no hay variable de entorno
      apiBaseUrl: 'https://localhost:7114'
    }
  },

  vite:{
    optimizeDeps: {
      include: [
        'pdfjs-dist',
        '@vue/devtools-core',
        '@vue/devtools-kit'
      ]
    },
    build: {
      target: 'esnext'
    }
  },

  devtools: {
    enabled: true
  },

  css: ['~/assets/css/main.css'],

  compatibilityDate: '2025-01-15',

  eslint: {
    config: {
      stylistic: {
        commaDangle: 'never',
        braceStyle: '1tbs'
      }
    }
  }
})
