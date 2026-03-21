var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.SmartExtractor_Api>("smart-extractor-api")
    .WithHttpHealthCheck("/health");

var apiUrl = apiService.GetEndpoint("https");

builder.AddJavaScriptApp("frontend", "../UI/smart-extractor")
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WithEnvironment("NUXT_PUBLIC_API_BASE_URL", apiUrl)
    .WithReference(apiService); ;

builder.Build().Run();