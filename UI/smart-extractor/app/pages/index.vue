<script setup lang="ts">
import { usePdfExtraction } from '../composables/usePdfExtraction'

const selectedPdfFile = ref<File | null>(null)
const userPrompt = ref('')
const {
  extractPdfTables,
  feedbackMessage: exportFeedbackMessage,
  isParsing,
  resetFeedback
} = usePdfExtraction()

const onFileSelected = (file: File) => {
  selectedPdfFile.value = file
  resetFeedback('PDF cargado. Presiona "Export to Excel" para iniciar el parseo.')
}

const onExportClicked = async () => {
  if (!selectedPdfFile.value || isParsing.value) {
    return
  }

  await extractPdfTables(selectedPdfFile.value, userPrompt.value)
}

const onPromptSubmitted = async () => {
  if (!selectedPdfFile.value || isParsing.value) {
    return
  }

  await extractPdfTables(selectedPdfFile.value, userPrompt.value)
}

const exportLabel = computed(() => {
  if (isParsing.value) {
    return 'Parsing...'
  }

  return 'Export to Excel'
})

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
          <DropZone @file-selected="onFileSelected" />
          <UChatPrompt
            v-model="userPrompt"
            placeholder="Puedes escribir aqui filtros o instrucciones para la exportacion..."
            class="rounded-2xl border border-outline-variant/40 bg-surface-container-high/80 text-on-surface neon-glow-primary"
            :disabled="!selectedPdfFile || isParsing"
            @submit="onPromptSubmitted"
          >
            <UChatPromptSubmit class="bg-primary! text-on-primary! hover:bg-primary-fixed-dim!" />
          </UChatPrompt>

          <div class="flex justify-between items-end px-1">
      <div>
        <h3 class="font-headline font-bold text-lg text-on-surface">
          Real-time Extraction
        </h3>
        <p class="font-label text-[10px] uppercase tracking-widest text-on-surface-variant">
          Live Parser Stream v2.0.4
        </p>
      </div>
      <button
        type="button"
        class="flex items-center gap-2 border border-outline-variant/20 px-4 py-2 rounded-xl transition-all"
        :class="[
          Boolean(selectedPdfFile) && !isParsing
            ? 'bg-surface-container-high hover:bg-surface-variant text-on-surface'
            : 'bg-surface-container-low text-on-surface-variant cursor-not-allowed opacity-70'
        ]"
        :disabled="!Boolean(selectedPdfFile) || isParsing"
        @click="onExportClicked"
      >
        <span class="material-symbols-outlined text-sm">download</span>
        <span class="font-label text-xs uppercase tracking-widest">{{ exportLabel }}</span>
      </button>
    </div>
    <p
      class="font-body text-sm px-1"
      :class="isParsing ? 'text-primary' : 'text-on-surface-variant'"
    >
      {{ exportFeedbackMessage || 'Sube un PDF para habilitar la exportación.' }}
    </p>
          <!-- <ResultsTable
            :has-pdf="Boolean(selectedPdfFile)"
            :is-parsing="isParsing"
            :feedback-message="exportFeedbackMessage"
            @export-clicked="onExportClicked"
          /> -->
        </section>

        <!-- Right Column: PDF Preview -->
        <PdfPreview :pdf-file="selectedPdfFile" />
      </div>
    </main>
  </div>
</template>