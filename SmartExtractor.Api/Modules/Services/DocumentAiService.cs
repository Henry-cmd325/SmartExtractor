using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace SmartExtractor.Api.Modules.Services
{
    public record TableResponse(string Name, List<List<string?>> Rows);
    public class DocumentAiService(Kernel kernel)
    {
        public async Task<List<TableResponse>> ExtraerTablasDesdeImagen(byte[] imageBytes)
        {
            var chatService = kernel.GetRequiredService<IChatCompletionService>();

            // El prompt debe ser MUY específico para que no te mande texto basura
            var history = new ChatHistory("""
                ROLE: High-precision data extraction expert.
                TASK: Analyze the provided image and extract ALL tables into a structured JSON format.
    
                CONSTRAINTS:
                1. Return ONLY a valid JSON object. No conversational text, no markdown code blocks (```json).
                2. Adhere to this SCHEMA: 
                   {
                     [
                       {
                         "name": "string (descriptive name)",
                         "rows": [ ["cell1", "cell2"], ["data1", "data2"] ]
                       }
                     ]
                   }
                3. Use null for empty cells. Do not skip columns.
                4. If a value is unreadable, use "UNCERTAIN".
                5. Maintain visual alignment: ensure row data matches the correct column headers.
                """);

            // Agregamos el mensaje del usuario con la imagen
            history.AddUserMessage(
            [
                new TextContent("Extrae las tablas de esta imagen en formato JSON:"),
                new ImageContent(imageBytes, "image/png")
            ]);

            var result = await chatService.GetChatMessageContentAsync(history);

            // Limpiamos el Markdown si el modelo lo agrega por error
            var cleared = result.Content?
                .Replace("```json", "")
                .Replace("```", "")
                .Trim() ?? string.Empty;


            try
            {
                // Deserializamos directamente a una LISTA
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<TableResponse>>(cleared, opciones) ?? [];
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return [];
            }
        }
    }
}
