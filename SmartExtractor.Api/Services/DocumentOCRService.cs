using Azure;
using Azure.AI.DocumentIntelligence;

namespace SmartExtractor.Api.Services
{
    public record TableResponse(int Id, int PageNumber, string Name, List<List<string?>> Rows);

    public class DocumentOCRService(DocumentIntelligenceClient client, IConfiguration configuration, ILogger<DocumentOCRService> logger)
    {
        private readonly string modelId = configuration["DocumentIntelligence:ModelId"] ?? "prebuilt-layout";

        public async Task<List<TableResponse>> ExtraerTablasPdf(byte[] imageBytes, CancellationToken cancellationToken = default)
        {
            if (imageBytes.Length == 0)
            {
                return [];
            }

            try
            {
                var operation = await client.AnalyzeDocumentAsync(
                    WaitUntil.Completed,
                    modelId,
                    BinaryData.FromBytes(imageBytes),
                    cancellationToken: cancellationToken);

                return ConvertirTablas(operation.Value.Tables);
            }
            catch (RequestFailedException ex)
            {
                logger.LogError(ex, "Error al analizar la imagen con Azure Document Intelligence.");
                throw new InvalidOperationException("No se pudo procesar el documento con Azure Document Intelligence.", ex);
            }
        }

        private static List<TableResponse> ConvertirTablas(IReadOnlyList<DocumentTable> tablas)
        {
            var tablasConvertidas = new List<TableResponse>(tablas.Count);

            for (var indiceTabla = 0; indiceTabla < tablas.Count; indiceTabla++)
            {
                var tabla = tablas[indiceTabla];
                var pageNumber = tabla.BoundingRegions.Count > 0
                    ? tabla.BoundingRegions[0].PageNumber
                    : 1;
                var filas = Enumerable.Range(0, tabla.RowCount)
                    .Select(_ => Enumerable.Repeat<string?>(null, tabla.ColumnCount).ToList())
                    .ToList();

                foreach (var celda in tabla.Cells)
                {
                    filas[celda.RowIndex][celda.ColumnIndex] = LimpiarContenido(celda.Content);
                }

                if (filas.Count > 0)
                {
                    tablasConvertidas.Add(new TableResponse(indiceTabla + 1, pageNumber, $"Tabla {indiceTabla + 1}", filas));
                }
            }

            return tablasConvertidas;
        }

        private static string? LimpiarContenido(string? contenido)
        {
            return string.IsNullOrWhiteSpace(contenido) ? null : contenido.Trim();
        }
    }
}
