<script setup lang="ts">
const emit = defineEmits<{
  'file-selected': [file: File]
}>()

const fileInput = ref<HTMLInputElement | null>(null)
const isDragging = ref(false)
const selectedFileName = ref('')
const errorMessage = ref('')

const isPdfFile = (file: File) => {
  if (file.type === 'application/pdf') {
    return true
  }

  return file.name.toLowerCase().endsWith('.pdf')
}

const handleValidFile = (file: File | undefined) => {
  if (!file) {
    return
  }

  if (!isPdfFile(file)) {
    errorMessage.value = 'Solo se permiten archivos PDF.'
    selectedFileName.value = ''

    if (fileInput.value) {
      fileInput.value.value = ''
    }

    return
  }

  errorMessage.value = ''
  selectedFileName.value = file.name
  emit('file-selected', file)
}

const openFileDialog = () => {
  fileInput.value?.click()
}

const onFileInputChange = (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  handleValidFile(file)
}

const onDragEnter = (event: DragEvent) => {
  event.preventDefault()
  isDragging.value = true
}

const onDragOver = (event: DragEvent) => {
  event.preventDefault()
  isDragging.value = true
}

const onDragLeave = (event: DragEvent) => {
  event.preventDefault()
  isDragging.value = false
}

const onDrop = (event: DragEvent) => {
  event.preventDefault()
  isDragging.value = false

  const file = event.dataTransfer?.files?.[0]
  handleValidFile(file)
}

</script>

<template>
  <div class="relative group">
    <div class="pointer-events-none absolute -inset-1 bg-linear-to-r from-primary/20 to-secondary/20 rounded-xl blur opacity-25 group-hover:opacity-50 transition duration-1000 group-hover:duration-200" />
    <div
      class="relative flex flex-col items-center justify-center border-2 border-dashed border-outline-variant/30 bg-surface-container-low rounded-xl p-12 transition-all duration-300 hover:border-primary/40"
      :class="{ 'border-primary/70 bg-surface-container': isDragging }"
      @dragenter="onDragEnter"
      @dragover="onDragOver"
      @dragleave="onDragLeave"
      @drop="onDrop"
      @click="openFileDialog"
    >
      <input
        ref="fileInput"
        type="file"
        accept="application/pdf,.pdf"
        class="hidden"
        @change="onFileInputChange"
      >
      <div class="w-16 h-16 rounded-full bg-surface-container-high flex items-center justify-center mb-4 neon-glow-primary">
        <span class="material-symbols-outlined text-primary text-3xl">upload_file</span>
      </div>
      <h2 class="font-headline font-bold text-xl mb-2 text-on-surface">
        Drop your PDF
      </h2>
      <p class="font-body text-on-surface-variant text-sm mb-6 text-center max-w-xs">
        Drag and drop your financial reports, invoices, or any PDF with tables for instant extraction.
      </p>
      <button
        type="button"
        class="bg-linear-to-br from-primary to-primary-dim text-on-primary-fixed font-headline font-bold px-8 py-3 rounded-xl transition-all active:scale-95 shadow-lg shadow-primary/20"
        @click.stop="openFileDialog"
      >
        Upload File
      </button>
      <p
        v-if="selectedFileName"
        class="mt-4 font-body text-sm text-on-surface"
      >
        Archivo seleccionado: {{ selectedFileName }}
      </p>
      <p
        v-if="errorMessage"
        class="mt-4 font-body text-sm text-error"
      >
        {{ errorMessage }}
      </p>
    </div>
  </div>
</template>
