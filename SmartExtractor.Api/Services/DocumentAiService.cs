using GenerativeAI;
using GenerativeAI.Types;
using System.Linq.Dynamic.Core;
using System.Text.Json;

namespace SmartExtractor.Api.Services
{
    public class DocumentAiService(GenerativeModel client, ILogger<DocumentAiService> logger)
    {
        public record TableMetaData(int Id, int PageNumber, string TableName, List<string> Columns);
        public record TableSelection(int TableId, string FilterExpression, string Reason);

        public async Task<List<TableResponse>> TransformarDatos(List<TableResponse> tables, string userPrompt)
        {
            if (string.IsNullOrWhiteSpace(userPrompt))
            {
                return [];
            }

            var json = SerializeMetadata(tables);

            // El prompt debe ser MUY específico para que no te mande texto basura
            var systemPrompt = """
                ROLE: Data Orchestrator and C# Dynamic LINQ Generator.
                TASK: Analyze the provided table metadata and the user's natural language request to generate a precise filtering plan.

                CONSTRAINTS & RULES:
                1. OUTPUT FORMAT: You MUST return ONLY a valid JSON object. No markdown formatting (```json), no conversational text.
                2. DYNAMIC LINQ SYNTAX: The filter must be written for the System.Linq.Dynamic.Core library. 
                   - The row data is represented as a string array called `it`. 
                   - Use indices based on the provided metadata (e.g., `it[0]`, `it[1]`).
                3. HEADER & NULL SAFETY: 
                    - ALWAYS verify the value is not the column header name before parsing.
                    - Use: `it[X] == "ColumnName" || !string.IsNullOrWhiteSpace(it[X])`
                4. TYPE CONVERSION: Use Convert methods or explicit parsing supported by Dynamic LINQ.
                    - Example (Age > 29): `it[3] == "Edad" || Int32.Parse(it[3]) > 29`
                    - Example (Salary > 500): `it[2] == "Sueldo" || Double.Parse(it[2]) > 500`
                5. SCHEMA REQUIREMENT: Your output must strictly adhere to this JSON structure:
                     [
                       {
                         "tableId": 0 (the tableId of the table that is going to be filtered),
                         "filterExpression": "string (the Dynamic LINQ condition)",
                         "reason": "string (brief explanation of the logic)"
                       }
                     ]
                """;

            var finalPrompt = $$"""
                DOCUMENT METADATA:
                {{json}}

                USER REQUEST:
                "{{userPrompt}}"

                Based on the USER REQUEST, identify the relevant tables from the METADATA and generate the JSON array.
            """;

            var request = new GenerateContentRequest() 
            { 
                GenerationConfig = new() { ResponseMimeType = "application/json" },
                SystemInstruction = new() 
                { 
                    Parts = [new(systemPrompt)]
                }, 
                Contents = 
                [
                    new() 
                    { 
                        Parts = [new() { Text = finalPrompt }] 
                    }
                ] 
            }; 

            var response = await client.GenerateContentAsync(request);

            // Limpiamos el Markdown si el modelo lo agrega por error
            var cleared = response.Text
                .Replace("```json", "")
                .Replace("```", "")
                .Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(cleared))
            {
                return [];
            }

            try
            {
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var deserialized = JsonSerializer.Deserialize<List<TableSelection>>(cleared, opciones)
                    ?? TryDeserializeWrappedResponse(cleared, opciones);

                if (deserialized == null)
                {
                    logger.LogWarning("La respuesta del modelo no se pudo deserializar en la estructura esperada. Respuesta: {Response}", cleared); 
                    return [];
                }

                return ApplySelections(tables, deserialized);
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "La respuesta del modelo no se pudo deserializar. Respuesta: {Response}", cleared);
                return [];
            }
        }

        private List<TableResponse> ApplySelections(List<TableResponse> tables, List<TableSelection> selections)
        {
            var tablasFiltradas = new List<TableResponse>(selections.Count);

            foreach (var selection in selections)
            {
                var table = tables.FirstOrDefault(t => t.Id == selection.TableId);

                if (table is null)
                {
                    logger.LogWarning("No se encontró la tabla con Id {TableId} en la colección proporcionada.", selection.TableId);
                    continue;
                }

                List<List<string?>> filasFiltradas;

                try
                {
                    filasFiltradas = string.IsNullOrWhiteSpace(selection.FilterExpression)
                        ? [.. table.Rows]
                        : [.. table.Rows
                            .AsQueryable()
                            .Where(selection.FilterExpression)];
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "No se pudo aplicar el filtro dinámico a la tabla con Id {TableId}.", selection.TableId);
                    continue;
                }

                tablasFiltradas.Add(table with { Rows = filasFiltradas });
            }

            return tablasFiltradas;
        }

        private static string SerializeMetadata(List<TableResponse> tables)
        {
            var metadata = tables.Select(table => new TableMetaData(
                table.Id,
                table.PageNumber,
                table.Name,
                table.Rows.FirstOrDefault()?.Select(cell => cell ?? string.Empty).ToList() ?? []));

            return JsonSerializer.Serialize(metadata);
        }

        private static List<TableSelection>? TryDeserializeWrappedResponse(string content, JsonSerializerOptions options)
        {
            using var document = JsonDocument.Parse(content);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return [];
            }

            foreach (var propertyName in new[] { "tables", "data", "result" })
            {
                if (document.RootElement.TryGetProperty(propertyName, out var property))
                {
                    return JsonSerializer.Deserialize<List<TableSelection>>(property.GetRawText(), options);
                }
            }

            return null;
        }
    }
}
