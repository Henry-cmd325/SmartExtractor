using GenerativeAI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using SmartExtractor.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddAntiforgery();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var geminiApiKey = builder.Configuration["GEMINI_KEY"];
var geminiModelId = builder.Configuration["Gemini:ModelId"]!;

if (string.IsNullOrWhiteSpace(geminiApiKey))
{
    throw new InvalidOperationException("Falta configurar la clave `GEMINI_KEY`.");
}

builder.Services.AddKernel()
    .AddGoogleAIGeminiChatCompletion(
        modelId: geminiModelId,
        apiKey: geminiApiKey,
        apiVersion: GoogleAIVersion.V1
    );

var client = new GenerativeModel(apiKey: geminiApiKey, model: geminiModelId);
builder.Services.AddScoped(_ => client);

builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<ExcelService>();
builder.Services.AddScoped<DocumentAiService>();

var app = builder.Build();

//Aqui van los endpoints de la API:
app.MapPost("/extract-tables", async (
    [FromForm] IFormFile pdf,
    PdfService pdfService,
    ExcelService excelService,
    DocumentAiService documentAiService) =>
{
    if (pdf.Length == 0)
    {
        return Results.BadRequest("Debes enviar un archivo PDF.");
    }

    if (!string.Equals(Path.GetExtension(pdf.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
    {
        return Results.BadRequest("El archivo enviado debe ser un PDF.");
    }

    await using var pdfStream = new MemoryStream();
    await pdf.CopyToAsync(pdfStream);
    var pdfBytes = pdfStream.ToArray();

    var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
    await File.WriteAllBytesAsync(tempFilePath, pdfBytes);

    try
    {
        var excelBytes = PdfService.ProcessPdfToExcel(tempFilePath);

        if (excelBytes is null)
        {
            var imagenes = pdfService.ConvertirPdfAImagenes(pdfBytes);
            var tablas = new List<TableResponse>();

            foreach (var imagen in imagenes)
            {
                var tablasExtraidas = await documentAiService.ExtraerTablasDesdeImagen(imagen);

                if (tablasExtraidas.Count > 0)
                {
                    tablas.AddRange(tablasExtraidas);
                }
            }

            if (tablas.Count == 0)
            {
                return Results.NotFound("No se encontraron tablas en el PDF.");
            }

            excelBytes = excelService.GenerarExcelDesdeTablas(tablas);
        }

        var outputFileName = $"{Path.GetFileNameWithoutExtension(pdf.FileName)}-tablas.xlsx";
        return Results.File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            outputFileName);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(
            title: "No se pudo procesar el PDF",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError);
    }
    finally
    {
        if (File.Exists(tempFilePath))
        {
            File.Delete(tempFilePath);
        }
    }
})
.Accepts<IFormFile>("multipart/form-data")
.Produces(StatusCodes.Status200OK, contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status500InternalServerError)
.DisableAntiforgery();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");

app.UseAntiforgery();

app.UseHttpsRedirection();

app.MapDefaultEndpoints();

app.Run();