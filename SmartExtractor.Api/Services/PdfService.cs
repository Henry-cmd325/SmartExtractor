using ClosedXML.Excel;
using Docnet.Core;
using Docnet.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Tabula;
using Tabula.Extractors;
using UglyToad.PdfPig;

namespace SmartExtractor.Api.Services
{
    public class PdfService
    {
        private static readonly ParsingOptions PdfParsingOptions = new() { ClipPaths = true };

        private bool IsImageOnly(string filePath)
        {
            using var pdf = PdfDocument.Open(filePath, PdfParsingOptions);

            // Analizamos la primera página como muestra
            var page = pdf.GetPage(1);

            // Si el conteo de letras es 0, es 100% una imagen/escaneo.
            // Si hay muy pocas letras (ej. < 10), podría ser un logo 
            // y el resto de la tabla ser una imagen.
            return !page.Letters.Any();
        }

        public int GetTotalPages(string filePath)
        {
            using var pdf = PdfDocument.Open(filePath, PdfParsingOptions);
            return pdf.NumberOfPages;
        }

        public List<Table> ExtraerTodasLasTablas(string rutaPdf)
        {
            var todasLasTablas = new List<Table>();
            var firmasVistas = new HashSet<string>(StringComparer.Ordinal);

            using (var document = PdfDocument.Open(rutaPdf, PdfParsingOptions))
            {
                var algorithm = new SpreadsheetExtractionAlgorithm();
                // Iteramos por cada página del PDF (PdfPig usa índice 1)
                for (int i = 1; i <= document.NumberOfPages; i++)
                {
                    // 1. Extraer el área de la página actual de forma estática
                    var pageArea = ObjectExtractor.Extract(document, i);

                    // 2. Usamos el algoritmo (puedes alternar entre Basic o SpreadSheet)

                    var tablasDePagina = algorithm.Extract(pageArea);

                    // 3. Agregamos las tablas encontradas a nuestra lista maestra
                    if (tablasDePagina != null && tablasDePagina.Count > 0)
                    {
                        foreach (var tabla in tablasDePagina)
                        {
                            var firma = CrearFirmaTabla(tabla);

                            if (!firmasVistas.Add(firma))
                            {
                                continue;
                            }

                            todasLasTablas.Add(tabla);
                        }
                    }
                }
            }

            return todasLasTablas;
        }

        private byte[] GenerarTablasEnExcel(IReadOnlyList<Table> tables)
        {
            using var libro = new XLWorkbook();
            int contadorHoja = 1;

            foreach (var tabla in tables)
            {
                var hoja = libro.Worksheets.Add($"Tabla {contadorHoja++}");
                int filaExcel = 1;

                var filasLimpias = tabla.Rows.Where(fila =>
                    // Solo dejamos filas donde al menos el 40% de las celdas tengan contenido
                    fila.Count(celda => !string.IsNullOrWhiteSpace(celda.GetText())) > (fila.Count * 0.4)
                ).ToList();

                foreach (var filaPdf in filasLimpias)
                {
                    int columnaExcel = 1;
                    foreach (var celdaPdf in filaPdf)
                    {
                        hoja.Cell(filaExcel, columnaExcel).Value = celdaPdf.GetText().Trim();
                        columnaExcel++;
                    }
                    filaExcel++;
                }
                hoja.Columns().AdjustToContents();
            }

            using var ms = new MemoryStream();
            libro.SaveAs(ms);
            return ms.ToArray();
        }

        public byte[]? ProcessPdfToExcel(string path)
        {
            if (IsImageOnly(path))
            {
                Console.WriteLine("⚠️ No se detectó texto. Enviando a Azure Document Intelligence...");
                return null;
            }
            else
            {
                var tables = ExtraerTodasLasTablas(path);
                if (tables != null)
                {
                    return GenerarTablasEnExcel(tables);
                }
            }

            Console.WriteLine("⚠️ No se pudo parsear. Enviando a Azure Document Intelligence...");
            return null;
        }

        public List<TableResponse> ProcessPdfToTableResponses(string path)
        {
            if (IsImageOnly(path))
            {
                Console.WriteLine("⚠️ No se detectó texto. Enviando a Azure Document Intelligence...");
                return [];
            }

            var tableResponses = new List<TableResponse>();
            var firmasVistas = new HashSet<string>(StringComparer.Ordinal);
            var algorithm = new SpreadsheetExtractionAlgorithm();
            var tableId = 1;

            using (var document = PdfDocument.Open(path, PdfParsingOptions))
            {
                for (int pageNumber = 1; pageNumber <= document.NumberOfPages; pageNumber++)
                {
                    var pageArea = ObjectExtractor.Extract(document, pageNumber);
                    var tables = algorithm.Extract(pageArea);

                    if (tables == null || tables.Count == 0)
                    {
                        continue;
                    }

                    foreach (var table in tables)
                    {
                        var rows = ConvertirFilas(table);

                        if (rows.Count == 0)
                        {
                            continue;
                        }

                        var firma = CrearFirmaTabla(rows);

                        if (!firmasVistas.Add(firma))
                        {
                            continue;
                        }

                        tableResponses.Add(new TableResponse(tableId, pageNumber, $"Tabla {tableId}", rows));
                        tableId++;
                    }
                }
            }

            if (tableResponses.Count > 0)
            {
                return tableResponses;
            }

            Console.WriteLine("⚠️ No se pudo parsear. Enviando a Azure Document Intelligence...");
            return [];
        }

        public List<byte[]> ConvertirPdfAImagenes(byte[] pdfBytes)
        {
            var imagenesBytes = new List<byte[]>();

            var dimensiones = new PageDimensions(2000, 2000);
            using var documentReader = DocLib.Instance.GetDocReader(pdfBytes, dimensiones);

            for (var indicePagina = 0; indicePagina < documentReader.GetPageCount(); indicePagina++)
            {
                using var pageReader = documentReader.GetPageReader(indicePagina);
                var pixeles = pageReader.GetImage();
                var ancho = pageReader.GetPageWidth();
                var alto = pageReader.GetPageHeight();

                using var imagen = Image.LoadPixelData<Bgra32>(pixeles, ancho, alto);
                using var stream = new MemoryStream();
                imagen.SaveAsPng(stream);
                imagenesBytes.Add(stream.ToArray());
            }

            return imagenesBytes;
        }

        private static List<List<string?>> ConvertirFilas(Table table)
        {
            return [.. table.Rows.Where(fila =>
                fila.Count(celda => !string.IsNullOrWhiteSpace(celda.GetText())) > (fila.Count * 0.4)
            )
            .Select(fila => fila
                .Select(celda =>
                {
                    var contenido = celda.GetText().Trim();
                    return string.IsNullOrWhiteSpace(contenido) ? null : contenido;
                })
                .ToList())];
        }

        private static string CrearFirmaTabla(Table table)
        {
            return CrearFirmaTabla(ConvertirFilas(table));
        }

        private static string CrearFirmaTabla(List<List<string?>> rows)
        {
            return string.Join("||", rows.Select(fila =>
                string.Join("|", fila.Select(celda => (celda ?? string.Empty).Trim()))));
        }
    }
}
