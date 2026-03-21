<script setup lang="ts">
useSeoMeta({
  title: 'Dashboard'
})
</script>

<template>
  <div class="flex h-screen overflow-hidden bg-surface">
    <AppSidebar />

    <main class="flex-1 ml-20 flex flex-col overflow-hidden">
      <AppHeader />

      <!-- Content Area: Asymmetric Layout -->
      <div class="flex-1 flex gap-0 overflow-hidden">
        <!-- Left Column: Interaction Zone -->
        <section class="flex-1 overflow-y-auto px-8 py-6 space-y-8">
          <DropZone />
          <TerminalInput />
          <ResultsTable />
        </section>

        <!-- Right Column: PDF Preview -->
        <PdfPreview />
      </div>
    </main>
  </div>
</template>
