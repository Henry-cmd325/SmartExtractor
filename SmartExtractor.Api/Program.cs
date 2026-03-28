using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.AspNetCore.Mvc;
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

var documentIntelligenceEndpoint = builder.Configuration["DocumentIntelligence:Endpoint"];
var documentIntelligenceApiKey = builder.Configuration["DocumentIntelligence:ApiKey"];

if (string.IsNullOrWhiteSpace(documentIntelligenceEndpoint) || string.IsNullOrWhiteSpace(documentIntelligenceApiKey))
{
    throw new InvalidOperationException("Falta configurar `DocumentIntelligence:Endpoint` o `DocumentIntelligence:ApiKey`.");
}

builder.Services.AddSingleton(_ => new DocumentIntelligenceClient(
    new Uri(documentIntelligenceEndpoint),
    new AzureKeyCredential(documentIntelligenceApiKey))
);

builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<ExcelService>();
builder.Services.AddScoped<DocumentOCRService>();

var app = builder.Build();

//Aqui van los endpoints de la API:
app.MapPost("/extract-tables", async (
    [FromForm] IFormFile pdf,
    PdfService pdfService,
    ExcelService excelService,
    DocumentOCRService documentOCRService,
    DocumentAiService documentAiService,
    CancellationToken cancellationToken,
    [FromQuery] string userPrompt) =>
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
    await pdf.CopyToAsync(pdfStream, cancellationToken);
    var pdfBytes = pdfStream.ToArray();

    var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
    await File.WriteAllBytesAsync(tempFilePath, pdfBytes, cancellationToken);

    if (pdfService.GetTotalPages(tempFilePath) > 20)
        return Results.BadRequest("El limite de páginas por PDF es 20. Por favor, reduce el número de páginas e inténtalo de nuevo.");

    try
    {
        var tablas = pdfService.ProcessPdfToTableResponses(tempFilePath);
        byte[] excelBytes = [];
        if (tablas.Count == 0)
        {
            tablas.AddRange(await documentOCRService.ExtraerTablasPdf(pdfBytes, cancellationToken));

            if (tablas.Count == 0)
            {
                return Results.NotFound("No se encontraron tablas en el PDF.");
            }
        }

        if (!string.IsNullOrWhiteSpace(userPrompt))
        {
            tablas = await documentAiService.TransformarDatos(tablas, userPrompt);

            if (tablas.Count == 0)
            {
                return Results.NotFound("No se encontraron resultados para el filtro solicitado.");
            }
        }

        excelBytes = excelService.GenerarExcelDesdeTablas(tablas);

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
            File.Delete(tempFilePath);
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