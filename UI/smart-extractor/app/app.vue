<script setup lang="ts">
useHead({
  htmlAttrs: {
    lang: 'es'
  },
  meta: [
    { name: 'viewport', content: 'width=device-width, initial-scale=1' }
  ],
  link: [
    { rel: 'icon', href: '/favicon.ico' },
    { rel: 'preconnect', href: 'https://fonts.googleapis.com' },
    { rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: '' },
    { rel: 'stylesheet', href: 'https://fonts.googleapis.com/css2?family=Manrope:wght@400;700;800&family=Inter:wght@400;500;600&family=Space+Grotesk:wght@400;500;700&display=swap' },
    { rel: 'stylesheet', href: 'https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:wght,FILL@100..700,0..1&display=swap' }
  ]
})

useSeoMeta({
  titleTemplate: titleChunk => titleChunk ? `${titleChunk} | Smart Extractor` : 'Smart Extractor',
  description: 'Interfaz de Smart Extractor.'
})
</script>

<template>
  <UApp>
    <NuxtPage />
  </UApp>
</template>
