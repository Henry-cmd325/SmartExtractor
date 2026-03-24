using ClosedXML.Excel;

namespace SmartExtractor.Api.Services
{
    public class ExcelService
    {
        public byte[] GenerarExcelDesdeTablas(List<TableResponse> tablas)
        {
            using var workbook = new XLWorkbook();

            foreach (var tabla in tablas)
            {
                // 1. Limpiamos el nombre de la hoja (máximo 31 caracteres y sin caracteres especiales)
                var sheetName = string.IsNullOrWhiteSpace(tabla.Name) ? "Tabla Extraida" : tabla.Name;
                var ws = workbook.Worksheets.Add(sheetName[..Math.Min(sheetName.Length, 31)]);

                // 2. Llenamos las filas
                for (int r = 0; r < tabla.Rows.Count; r++)
                {
                    var currentDataRow = tabla.Rows[r];
                    for (int c = 0; c < currentDataRow.Count; c++)
                    {
                        // r + 1 porque Excel empieza en 1, no en 0
                        ws.Cell(r + 1, c + 1).Value = currentDataRow[c] ?? string.Empty;
                    }
                }

                // 3. Formato "Pro" (Solo si la tabla tiene datos)
                if (tabla.Rows.Count > 0)
                {
                    // Ponemos la primera fila en negrita y con fondo gris claro (Encabezados)
                    var headerRow = ws.Row(1);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

                    // Ajustamos el ancho de las columnas automáticamente al contenido
                    ws.Columns().AdjustToContents();

                    // Agregamos bordes básicos a toda la tabla
                    var range = ws.Range(1, 1, tabla.Rows.Count, tabla.Rows[0].Count);
                    range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                }
            }

            // 4. Convertimos a array de bytes para mandarlo al Front o guardarlo
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
