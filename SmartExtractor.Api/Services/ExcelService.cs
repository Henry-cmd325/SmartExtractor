using ClosedXML.Excel;

namespace SmartExtractor.Api.Services
{
    public class ExcelService
    {
        public byte[] GenerarExcelDesdeTablas(List<TableResponse> tablas)
        {
            using var workbook = new XLWorkbook();

            var tablasAgrupadas = tablas
                .Select(tabla => new
                {
                    Tabla = tabla,
                    FilasLimpias = LimpiarFilas(tabla.Rows)
                })
                .Where(x => x.FilasLimpias.Count > 0)
                .GroupBy(x => ObtenerClaveDeMetadatos(x.FilasLimpias));

            foreach (var grupo in tablasAgrupadas)
            {
                var primeraTabla = grupo.First();
                var filasAgrupadas = new List<List<string?>>();

                foreach (var tablaAgrupada in grupo)
                {
                    if (filasAgrupadas.Count == 0)
                    {
                        filasAgrupadas.AddRange(tablaAgrupada.FilasLimpias.Select(fila => new List<string?>(fila)));
                        continue;
                    }

                    var filasParaAgregar = TieneMismoEncabezado(filasAgrupadas[0], tablaAgrupada.FilasLimpias[0])
                        ? tablaAgrupada.FilasLimpias.Skip(1)
                        : tablaAgrupada.FilasLimpias;

                    filasAgrupadas.AddRange(filasParaAgregar.Select(fila => new List<string?>(fila)));
                }

                // 1. Limpiamos el nombre de la hoja (máximo 31 caracteres y sin caracteres especiales)
                var baseSheetName = string.IsNullOrWhiteSpace(primeraTabla.Tabla.Name)
                    ? $"Tabla {primeraTabla.Tabla.Id}"
                    : primeraTabla.Tabla.Name;
                var sheetName = $"{baseSheetName}-Agrupada";
                var ws = workbook.Worksheets.Add(sheetName[..Math.Min(sheetName.Length, 31)]);

                // 2. Llenamos las filas
                for (int r = 0; r < filasAgrupadas.Count; r++)
                {
                    var currentDataRow = filasAgrupadas[r];
                    for (int c = 0; c < currentDataRow.Count; c++)
                    {
                        // r + 1 porque Excel empieza en 1, no en 0
                        ws.Cell(r + 1, c + 1).Value = currentDataRow[c] ?? string.Empty;
                    }
                }

                // 3. Formato "Pro" (Solo si la tabla tiene datos)
                if (filasAgrupadas.Count > 0)
                {
                    // Ponemos la primera fila en negrita y con fondo gris claro (Encabezados)
                    var headerRow = ws.Row(1);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

                    // Ajustamos el ancho de las columnas automáticamente al contenido
                    ws.Columns().AdjustToContents();

                    // Agregamos bordes básicos a toda la tabla
                    var totalColumnas = filasAgrupadas.Max(fila => fila.Count);
                    var range = ws.Range(1, 1, filasAgrupadas.Count, totalColumnas);
                    range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                }
            }

            // 4. Convertimos a array de bytes para mandarlo al Front o guardarlo
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static List<List<string?>> LimpiarFilas(List<List<string?>> filas)
        {
            return [.. filas.Where(fila =>
                fila.Count(celda => !string.IsNullOrWhiteSpace(celda)) > (fila.Count * 0.4)
            )];
        }

        private static string ObtenerClaveDeMetadatos(List<List<string?>> filas)
        {
            var encabezado = filas[0];
            var columnas = string.Join("|", encabezado.Select(celda => (celda ?? string.Empty).Trim().ToUpperInvariant()));
            return $"{encabezado.Count}:{columnas}";
        }

        private static bool TieneMismoEncabezado(List<string?> encabezadoActual, List<string?> encabezadoNuevo)
        {
            if (encabezadoActual.Count != encabezadoNuevo.Count)
            {
                return false;
            }

            for (var i = 0; i < encabezadoActual.Count; i++)
            {
                if (!string.Equals(encabezadoActual[i]?.Trim(), encabezadoNuevo[i]?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
