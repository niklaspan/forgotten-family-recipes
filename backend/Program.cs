using HandedDown.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        // Singleton because CosmosClient manages an internal connection pool and is explicitly
        // designed to be reused for the lifetime of the application — creating one per request
        // would exhaust TCP connections and introduce significant startup latency on every call.
        services.AddSingleton<IRecipeService, RecipeService>();
    })
    .Build();

host.Run();
