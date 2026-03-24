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
        private static bool IsImageOnly(string filePath)
        {
            using var pdf = PdfDocument.Open(filePath);

            // Analizamos la primera página como muestra
            var page = pdf.GetPage(1);

            // Si el conteo de letras es 0, es 100% una imagen/escaneo.
            // Si hay muy pocas letras (ej. < 10), podría ser un logo 
            // y el resto de la tabla ser una imagen.
            return !page.Letters.Any();
        }

        public static List<Table> ExtraerTodasLasTablas(string rutaPdf)
        {
            var todasLasTablas = new List<Table>();

            using (var document = PdfDocument.Open(rutaPdf))
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
                        todasLasTablas.AddRange(tablasDePagina);
                    }
                }
            }

            return todasLasTablas;
        }

        private static byte[] GenerarTablasEnExcel(IReadOnlyList<Table> tables)
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

        public static byte[]? ProcessPdfToExcel(string path)
        {
            if (IsImageOnly(path))
            {
                Console.WriteLine("⚠️ No se detectó texto. Enviando a IA (VLM)...");
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

            Console.WriteLine("⚠️ No se pudo parsear. Enviando a IA (VLM)...");
            return null;
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
    }
}
