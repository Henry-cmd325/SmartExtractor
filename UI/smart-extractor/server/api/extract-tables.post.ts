const forwardedHeaders = [
  'cache-control',
  'content-disposition',
  'content-type'
]

const buildExtractTablesUrl = (apiBaseUrl: string, userPrompt?: string) => {
  const requestUrl = new URL('/extract-tables', apiBaseUrl)
  const trimmedPrompt = userPrompt?.trim()

  if (trimmedPrompt) {
    requestUrl.searchParams.set('userPrompt', trimmedPrompt)
  }

  return requestUrl.toString()
}

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig(event)
  const parts = await readMultipartFormData(event)
  const pdfPart = parts?.find(part => part.name === 'pdf' && part.data)

  if (!pdfPart?.data) {
    throw createError({
      statusCode: 400,
      statusMessage: 'No se encontro el archivo PDF en la solicitud.'
    })
  }

  const formData = new FormData()
  formData.append(
    'pdf',
    new Blob([pdfPart.data], { type: pdfPart.type || 'application/pdf' }),
    pdfPart.filename || 'documento.pdf'
  )

  const query = getQuery(event)
  const userPrompt = typeof query.userPrompt === 'string'
    ? query.userPrompt
    : undefined

  let upstreamResponse: Response

  try {
    upstreamResponse = await fetch(buildExtractTablesUrl(config.public.apiBaseUrl, userPrompt), {
      method: 'POST',
      body: formData
    })
  } catch {
    throw createError({
      statusCode: 502,
      statusMessage: 'No se pudo contactar la API de extraccion.'
    })
  }

  const responseHeaders = new Headers()

  for (const headerName of forwardedHeaders) {
    const headerValue = upstreamResponse.headers.get(headerName)

    if (headerValue) {
      responseHeaders.set(headerName, headerValue)
    }
  }

  return new Response(upstreamResponse.body, {
    status: upstreamResponse.status,
    statusText: upstreamResponse.statusText,
    headers: responseHeaders
  })
})