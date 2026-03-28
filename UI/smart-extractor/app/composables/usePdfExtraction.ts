const extractTablesEndpoint = `${useRuntimeConfig().public.apiBaseUrl}/extract-tables`
const extractionToastId = 'pdf-extraction-toast'

const excelMimeType = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'

const getDownloadFileName = (response: Response, fallbackFileName: string) => {
  const contentDisposition = response.headers.get('content-disposition')

  if (!contentDisposition) {
    return fallbackFileName
  }

  const utf8FileNameMatch = contentDisposition.match(/filename\*=UTF-8''([^;]+)/i)

  if (utf8FileNameMatch?.[1]) {
    return decodeURIComponent(utf8FileNameMatch[1])
  }

  const asciiFileNameMatch = contentDisposition.match(/filename="?([^";]+)"?/i)

  if (asciiFileNameMatch?.[1]) {
    return asciiFileNameMatch[1]
  }

  return fallbackFileName
}

const downloadBlobFile = (blob: Blob, fileName: string) => {
  const downloadUrl = URL.createObjectURL(blob)
  const link = document.createElement('a')

  link.href = downloadUrl
  link.download = fileName
  link.style.display = 'none'

  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)

  URL.revokeObjectURL(downloadUrl)
}

export const usePdfExtraction = () => {
  const toast = useToast()
  const isParsing = ref(false)
  const feedbackMessage = ref('')
  const lastDownload = ref<{ blob: Blob, fileName: string } | null>(null)

  const downloadLatestExcel = () => {
    if (!lastDownload.value) {
      feedbackMessage.value = 'Todavia no hay un archivo listo para descargar.'
      return
    }

    downloadBlobFile(lastDownload.value.blob, lastDownload.value.fileName)
    feedbackMessage.value = `Descargando ${lastDownload.value.fileName}...`
  }

  const showLoadingToast = () => {
    toast.add({
      id: extractionToastId,
      title: 'Procesando PDF',
      description: 'Extrayendo tablas y preparando el Excel...',
      icon: 'i-lucide-loader-circle',
      color: 'primary',
      duration: 0,
      close: false,
      progress: false,
      ui: {
        icon: 'animate-spin'
      }
    })
  }

  const showSuccessToast = (fileName: string) => {
    toast.update(extractionToastId, {
      title: 'Excel listo',
      description: `El archivo ${fileName} ya deberia estarse descargando automaticamente. Si no inicio, puedes descargarlo manualmente aqui.`,
      icon: 'i-lucide-badge-check',
      color: 'success',
      duration: 0,
      close: {
        color: 'neutral',
        variant: 'link'
      },
      progress: false,
      actions: [{
        label: 'Descargar Excel',
        icon: 'i-lucide-download',
        color: 'success',
        variant: 'soft',
        onClick: (event) => {
          event?.stopPropagation()
          downloadLatestExcel()
        }
      }],
      ui: {
        root: 'ring ring-success/30 bg-elevated',
        title: 'text-highlighted',
        description: 'text-muted'
      }
    })
  }

  const showErrorToast = (message: string) => {
    toast.update(extractionToastId, {
      title: 'No se pudo generar el Excel',
      description: message,
      icon: 'i-lucide-circle-alert',
      color: 'error',
      duration: 0,
      close: {
        color: 'neutral',
        variant: 'link'
      },
      progress: false,
      ui: {
        root: 'ring ring-error/25 bg-elevated'
      }
    })
  }

  const extractPdfTables = async (file: File) => {
    if (isParsing.value) {
      return
    }

    isParsing.value = true
    lastDownload.value = null
    feedbackMessage.value = 'Enviando archivo para parseo...'
    showLoadingToast()

    const formData = new FormData()
    formData.append('pdf', file, file.name)

    try {
      const response = await fetch(extractTablesEndpoint, {
        method: 'POST',
        body: formData
      })

      if (!response.ok) {
        const errorBody = await response.text()
        throw new Error(errorBody || `La API respondio con estado ${response.status}.`)
      }

      const contentType = response.headers.get('content-type') || ''
      const excelFileName = `${file.name.replace(/\.pdf$/i, '') || 'resultado'}.xlsx`

      if (contentType.includes(excelMimeType) || contentType.includes('application/octet-stream')) {
        const blob = await response.blob()
        const fileName = getDownloadFileName(response, excelFileName)

        lastDownload.value = {
          blob,
          fileName
        }
        downloadLatestExcel()
        feedbackMessage.value = 'Excel generado y descargado correctamente.'
        showSuccessToast(fileName)
        return
      }

      const blob = await response.blob()

      if (blob.size > 0) {
        const fileName = getDownloadFileName(response, excelFileName)

        lastDownload.value = {
          blob,
          fileName
        }
        downloadLatestExcel()
        feedbackMessage.value = 'Archivo generado y descargado correctamente.'
        showSuccessToast(fileName)
        return
      }

      feedbackMessage.value = 'La API respondio correctamente, pero no devolvio un archivo para descargar.'
      toast.update(extractionToastId, {
        title: 'Proceso terminado',
        description: feedbackMessage.value,
        icon: 'i-lucide-file-warning',
        color: 'warning',
        duration: 0,
        progress: false,
        close: {
          color: 'neutral',
          variant: 'link'
        }
      })
    }
    catch (error) {
      const message = error instanceof Error
        ? error.message
        : 'No se pudo enviar el PDF a la API de extraccion.'

      feedbackMessage.value = `Error al procesar el PDF: ${message}`
      showErrorToast(feedbackMessage.value)
    }
    finally {
      isParsing.value = false
    }
  }

  const resetFeedback = (message = '') => {
    feedbackMessage.value = message
  }

  return {
    extractPdfTables,
    feedbackMessage,
    isParsing,
    downloadLatestExcel,
    resetFeedback
  }
}