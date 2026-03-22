<script setup lang="ts">
const selectedPdfFile = ref<File | null>(null)
const isParsing = ref(false)
const exportFeedbackMessage = ref('')

const onFileSelected = (file: File) => {
  selectedPdfFile.value = file
  isParsing.value = false
  exportFeedbackMessage.value = 'PDF cargado. Presiona "Export to Excel" para iniciar el parseo.'
}

const onExportClicked = async () => {
  if (!selectedPdfFile.value || isParsing.value) {
    return
  }

  isParsing.value = true
  exportFeedbackMessage.value = 'Enviando archivo para parseo...'

  await new Promise(resolve => setTimeout(resolve, 1200))

  isParsing.value = false
  exportFeedbackMessage.value = 'Parseo iniciado. En breve podrás exportar el resultado a Excel.'
}

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
          <!-- <UChatPrompt placeholder="Puedes escribir como quieres que se exporten tus datos">
            <UChatPromptSubmit />
          </UChatPrompt> -->
          <ResultsTable
            :has-pdf="Boolean(selectedPdfFile)"
            :is-parsing="isParsing"
            :feedback-message="exportFeedbackMessage"
            @export-clicked="onExportClicked"
          />
        </section>

        <!-- Right Column: PDF Preview -->
        <PdfPreview :pdf-file="selectedPdfFile" />
      </div>
    </main>
  </div>
</template>
