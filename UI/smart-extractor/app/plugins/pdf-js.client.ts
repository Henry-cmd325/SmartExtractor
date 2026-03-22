import * as pdfjs from 'pdfjs-dist/legacy/build/pdf.mjs'
import pdfWorker from 'pdfjs-dist/legacy/build/pdf.worker.min.mjs?url'

export default defineNuxtPlugin(() => {
  pdfjs.GlobalWorkerOptions.workerSrc = pdfWorker

  return {
    provide: {
      pdfjs
    }
  }
})