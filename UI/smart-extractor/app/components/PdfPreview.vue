<script setup lang="ts">
const props = defineProps<{
  pdfFile?: File | null
}>()

const { $pdfjs } = useNuxtApp()

type PdfjsClient = NonNullable<typeof $pdfjs>
type PdfLoadingTask = ReturnType<PdfjsClient['getDocument']>
type PdfDocument = Awaited<PdfLoadingTask['promise']>

const canvasRef = ref<HTMLCanvasElement | null>(null)
const previewScrollRef = ref<HTMLDivElement | null>(null)
const pdfDocument = shallowRef<PdfDocument | null>(null)
const currentPage = ref(1)
const totalPages = ref(0)
const zoomLevel = ref(1)
const isLoading = ref(false)
const isRendering = ref(false)
const previewError = ref('')

const canZoomOut = computed(() => zoomLevel.value > 1)
const canZoomIn = computed(() => zoomLevel.value < 4)
const hasPdf = computed(() => Boolean(pdfDocument.value))

const canGoToPreviousPage = computed(() => currentPage.value > 1)
const canGoToNextPage = computed(() => totalPages.value > 0 && currentPage.value < totalPages.value)

const progressWidth = computed(() => {
  if (!totalPages.value) {
    return '0%'
  }

  return `${(currentPage.value / totalPages.value) * 100}%`
})

const clearCanvas = () => {
  const canvas = canvasRef.value
  const context = canvas?.getContext('2d')

  if (!canvas || !context) {
    return
  }

  context.clearRect(0, 0, canvas.width, canvas.height)
}

const destroyPdfDocument = async () => {
  if (!pdfDocument.value) {
    return
  }

  await pdfDocument.value.destroy()
  pdfDocument.value = null
}

const renderCurrentPage = async () => {
  if (!pdfDocument.value || !canvasRef.value) {
    return
  }

  isRendering.value = true

  try {
    const page = await pdfDocument.value.getPage(currentPage.value)
    const baseViewport = page.getViewport({ scale: 1 })
    const availableWidth = Math.max(240, previewScrollRef.value?.clientWidth ?? baseViewport.width)
    const fitScale = availableWidth / baseViewport.width
    const renderScale = fitScale * zoomLevel.value
    const viewport = page.getViewport({ scale: renderScale })
    const devicePixelRatio = window.devicePixelRatio || 1
    const canvas = canvasRef.value
    const context = canvas.getContext('2d')

    if (!context) {
      return
    }

    canvas.style.width = `${viewport.width}px`
    canvas.style.height = `${viewport.height}px`
    canvas.width = Math.floor(viewport.width * devicePixelRatio)
    canvas.height = Math.floor(viewport.height * devicePixelRatio)

    context.setTransform(devicePixelRatio, 0, 0, devicePixelRatio, 0, 0)
    context.clearRect(0, 0, canvas.width, canvas.height)

    await page.render({
      canvas,
      canvasContext: context,
      viewport
    }).promise
  }
  finally {
    isRendering.value = false
  }
}

const loadPdfFile = async (file: File) => {
  if (!$pdfjs) {
    throw new Error('PDF.js no está disponible')
  }

  isLoading.value = true
  previewError.value = ''

  try {
    await destroyPdfDocument()
    const fileBuffer = await file.arrayBuffer()
    const loadingTask: PdfLoadingTask = $pdfjs.getDocument({ data: new Uint8Array(fileBuffer) })
    const loadedDocument = await loadingTask.promise

    pdfDocument.value = loadedDocument
    totalPages.value = loadedDocument.numPages
    currentPage.value = 1

    await nextTick()
    await renderCurrentPage()
  } finally {
    isLoading.value = false
  }
}

const resetPreview = async () => {
  await destroyPdfDocument()
  totalPages.value = 0
  currentPage.value = 1
  zoomLevel.value = 1
  previewError.value = ''
  clearCanvas()
}

const goToPreviousPage = () => {
  if (!canGoToPreviousPage.value) {
    return
  }

  currentPage.value -= 1
}

const goToNextPage = () => {
  if (!canGoToNextPage.value) {
    return
  }

  currentPage.value += 1
}

const zoomOut = () => {
  if (!canZoomOut.value) {
    return
  }

  const container = previewScrollRef.value
  const previousZoom = zoomLevel.value
  const nextZoom = Math.max(1, Number((zoomLevel.value - 0.25).toFixed(2)))

  if (!container || previousZoom === nextZoom) {
    zoomLevel.value = nextZoom
    return
  }

  const centerX = container.scrollLeft + container.clientWidth / 2
  const centerY = container.scrollTop + container.clientHeight / 2
  const ratio = nextZoom / previousZoom

  zoomLevel.value = nextZoom

  nextTick(() => {
    container.scrollLeft = Math.max(0, centerX * ratio - container.clientWidth / 2)
    container.scrollTop = Math.max(0, centerY * ratio - container.clientHeight / 2)
  })
}

const zoomIn = () => {
  if (!canZoomIn.value) {
    return
  }

  const container = previewScrollRef.value
  const previousZoom = zoomLevel.value
  const nextZoom = Math.min(4, Number((zoomLevel.value + 0.25).toFixed(2)))

  if (!container || previousZoom === nextZoom) {
    zoomLevel.value = nextZoom
    return
  }

  const centerX = container.scrollLeft + container.clientWidth / 2
  const centerY = container.scrollTop + container.clientHeight / 2
  const ratio = nextZoom / previousZoom

  zoomLevel.value = nextZoom

  nextTick(() => {
    container.scrollLeft = Math.max(0, centerX * ratio - container.clientWidth / 2)
    container.scrollTop = Math.max(0, centerY * ratio - container.clientHeight / 2)
  })
}

watch(() => props.pdfFile, async (file) => {
  await resetPreview()

  if (file) {
    try {
      await loadPdfFile(file)
    } catch (error) {
      const detail = error instanceof Error ? error.message : 'Error desconocido'
      previewError.value = `No se pudo cargar el PDF: ${detail}`
    }
  }
}, { immediate: true })

watch([currentPage, zoomLevel], async () => {
  if (!hasPdf.value || isLoading.value) {
    return
  }

  try {
    await renderCurrentPage()
  } catch (error) {
    const detail = error instanceof Error ? error.message : 'Error desconocido'
    previewError.value = `No se pudo renderizar la página: ${detail}`
  }
})

onBeforeUnmount(async () => {
  await destroyPdfDocument()
})
</script>

<template>
  <section class="w-112.5 bg-surface-container-low border-l border-outline-variant/15 flex flex-col">
    <div class="p-6 flex items-center justify-between border-b border-outline-variant/10">
      <div class="flex items-center gap-3">
        <span class="material-symbols-outlined text-primary">visibility</span>
        <div>
          <h3 class="font-headline font-bold text-sm uppercase tracking-wider text-on-surface">
            Live PDF Preview
          </h3>
          <p
            v-if="pdfFile"
            class="font-label text-[10px] text-on-surface-variant truncate max-w-56"
          >
            {{ pdfFile.name }}
          </p>
        </div>
      </div>
      <div class="flex items-center gap-2">
        <button
          type="button"
          class="p-1 rounded transition-colors text-on-surface"
          :class="canZoomIn ? 'hover:bg-surface-container-high' : 'opacity-40 cursor-not-allowed'"
          :disabled="!canZoomIn"
          @click="zoomIn"
        >
          <span class="material-symbols-outlined text-lg">zoom_in</span>
        </button>
        <button
          type="button"
          class="p-1 rounded transition-colors text-on-surface"
          :class="canZoomOut ? 'hover:bg-surface-container-high' : 'opacity-40 cursor-not-allowed'"
          :disabled="!canZoomOut"
          @click="zoomOut"
        >
          <span class="material-symbols-outlined text-lg">zoom_out</span>
        </button>
      </div>
    </div>
    <div class="flex-1 overflow-hidden p-0 bg-surface-container-lowest flex items-center justify-center">
      <div
        v-if="hasPdf"
        ref="previewScrollRef"
        class="w-full h-full overflow-auto bg-white text-on-surface [scrollbar-width:thin] [scrollbar-color:currentColor_transparent] [&::-webkit-scrollbar]:h-1.5 [&::-webkit-scrollbar]:w-1.5 [&::-webkit-scrollbar-track]:bg-transparent [&::-webkit-scrollbar-thumb]:rounded-full [&::-webkit-scrollbar-thumb]:bg-on-surface/70 hover:[&::-webkit-scrollbar-thumb]:bg-on-surface"
      >
        <div class="w-max min-w-full">
          <canvas
            ref="canvasRef"
            class="block bg-white max-w-none"
          />
        </div>
      </div>
      <div
        v-else-if="isLoading"
        class="w-full max-w-sm aspect-[1/1.4] bg-surface-container rounded-xl border border-outline-variant/20 flex flex-col items-center justify-center gap-3 text-on-surface-variant px-6 text-center"
      >
        <span class="material-symbols-outlined text-4xl text-primary animate-pulse">progress_activity</span>
        <p class="font-headline text-sm uppercase tracking-wider">
          Loading PDF
        </p>
      </div>
      <div
        v-else-if="previewError"
        class="w-full max-w-sm aspect-[1/1.4] bg-surface-container rounded-xl border border-outline-variant/20 flex flex-col items-center justify-center gap-3 text-error px-6 text-center"
      >
        <span class="material-symbols-outlined text-4xl">error</span>
        <p class="font-headline text-sm uppercase tracking-wider">
          Error loading preview
        </p>
        <p class="font-body text-sm text-on-surface-variant">
          {{ previewError }}
        </p>
      </div>
      <div
        v-else
        class="w-full max-w-sm aspect-[1/1.4] bg-surface-container rounded-xl border border-outline-variant/20 flex flex-col items-center justify-center gap-3 text-on-surface-variant px-6 text-center"
      >
        <span class="material-symbols-outlined text-4xl text-primary">description</span>
        <p class="font-headline text-sm uppercase tracking-wider">
          No PDF selected
        </p>
        <p class="font-body text-sm">
          Sube un archivo PDF para ver su vista previa aquí.
        </p>
      </div>
    </div>
    <div class="p-6 bg-surface-container-high border-t border-outline-variant/10">
      <div class="flex justify-between items-center mb-4">
        <span class="font-label text-[10px] text-on-surface-variant">
          PAGE {{ totalPages ? currentPage : 0 }} OF {{ totalPages }}
        </span>
        <div class="flex gap-1 text-on-surface">
          <button
            type="button"
            class="w-8 h-8 rounded border border-outline-variant/20 flex items-center justify-center"
            :class="canGoToPreviousPage ? 'hover:bg-surface-variant' : 'opacity-40 cursor-not-allowed'"
            :disabled="!canGoToPreviousPage"
            @click="goToPreviousPage"
          >
            <span class="material-symbols-outlined text-sm">chevron_left</span>
          </button>
          <button
            type="button"
            class="w-8 h-8 rounded border border-outline-variant/20 flex items-center justify-center"
            :class="canGoToNextPage ? 'hover:bg-surface-variant' : 'opacity-40 cursor-not-allowed'"
            :disabled="!canGoToNextPage"
            @click="goToNextPage"
          >
            <span class="material-symbols-outlined text-sm">chevron_right</span>
          </button>
        </div>
      </div>
      <div class="w-full bg-surface-container-lowest h-1.5 rounded-full overflow-hidden">
        <div
          class="h-full bg-primary-dim"
          :style="{ width: progressWidth }"
        />
      </div>
    </div>
  </section>
</template>
