<script setup lang="ts">
useSeoMeta({
  title: 'Inicio'
})
</script>

<template>
  <main class="min-h-screen bg-default">
    <UContainer class="flex min-h-screen items-center py-16">
      <section class="w-full rounded-2xl border border-default bg-muted/30 p-8 sm:p-10">
        <p class="text-sm font-medium uppercase tracking-[0.24em] text-muted">
          Smart Extractor
        </p>
        <h1 class="mt-4 text-4xl font-semibold text-highlighted sm:text-5xl">
          Proyecto limpio y listo para empezar.
        </h1>
        <p class="mt-4 max-w-2xl text-base leading-7 text-toned">
          Se eliminó la plantilla por defecto de Nuxt UI para dejar una base mínima sobre la que puedas construir.
        </p>
      </section>
    </UContainer>
  </main>
</template>
