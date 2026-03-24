using GenerativeAI;
using GenerativeAI.Types;
using System.Text.Json;

namespace SmartExtractor.Api.Services
{
    public record TableResponse(string Name, List<List<string?>> Rows);
    public class DocumentAiService(GenerativeModel client, ILogger<DocumentAiService> logger)
    {
        public async Task<List<TableResponse>> ExtraerTablasDesdeImagen(byte[] imageBytes)
        {
            if (imageBytes.Length == 0)
            {
                return [];
            }

            // El prompt debe ser MUY específico para que no te mande texto basura
            var systemPrompt = """
                ROLE: High-precision data extraction expert.
                TASK: Analyze the provided image and extract ALL tables into a structured JSON format.
    
                CONSTRAINTS:
                1. Return ONLY a valid JSON array. No conversational text, no markdown code blocks (```json).
                2. Adhere to this SCHEMA: 
                   [
                     {
                       "name": "string (descriptive name)",
                       "rows": [ ["cell1", "cell2"], ["data1", "data2"] ]
                     }
                   ]
                3. Use null for empty cells. Do not skip columns.
                4. If a value is unreadable, use "UNCERTAIN".
                5. Maintain visual alignment: ensure row data matches the correct column headers.
                """;

            var request = new GenerateContentRequest() 
            { 
                SystemInstruction = new() 
                { 
                    Parts = [new(systemPrompt)]
                }, 
                Contents = 
                [
                    new() 
                    { 
                        Parts = [new() { InlineData = new Blob { Data = Convert.ToBase64String(imageBytes), MimeType = "image/png"} }] 
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

                return JsonSerializer.Deserialize<List<TableResponse>>(cleared, opciones)
                    ?? TryDeserializeWrappedResponse(cleared, opciones);
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "La respuesta del modelo no se pudo deserializar. Respuesta: {Response}", cleared);
                return [];
            }
        }

        private static List<TableResponse> TryDeserializeWrappedResponse(string content, JsonSerializerOptions options)
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
                    return JsonSerializer.Deserialize<List<TableResponse>>(property.GetRawText(), options) ?? [];
                }
            }

            return [];
        }
    }
}
