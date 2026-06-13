using System.Text.Json;
using HandedDown.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // The Cosmos DB SDK uses CosmosNewtonsoftSerializer (which honours [JsonProperty])
        // for storage, but HTTP responses go through ASP.NET Core's MVC pipeline which uses
        // System.Text.Json. Without this, IActionResult serialises C# PascalCase property
        // names (e.g. "Id", "Title") and the frontend receives the wrong field names.
        services.Configure<JsonOptions>(options =>
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

        // Singleton because CosmosClient manages an internal connection pool and is explicitly
        // designed to be reused for the lifetime of the application — creating one per request
        // would exhaust TCP connections and introduce significant startup latency on every call.
        services.AddSingleton<IRecipeService, RecipeService>();
    })
    .Build();

host.Run();
