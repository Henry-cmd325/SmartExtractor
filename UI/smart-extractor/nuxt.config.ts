// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  devtools: {
    enabled: true
  },

  modules: [
    '@nuxt/eslint',
    '@nuxt/ui'
  ],

  runtimeConfig: {
    public: {
      apiBaseUrl: 'https://localhost:7114'
    }
  },

  vite: {
    optimizeDeps: {
      include: [
        'pdfjs-dist',
        'pdfjs-dist/legacy/build/pdf.mjs' ,
        '@vue/devtools-core',
        '@vue/devtools-kit'
      ]
    },
    build: {
      target: 'esnext'
    }
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
  },
  // Si usas Nuxt UI, a veces basta con esto:
  ui: {
    fonts: false 
  }
})
