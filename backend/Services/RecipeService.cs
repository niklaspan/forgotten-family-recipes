using System.Net;
using System.Text.RegularExpressions;
using HandedDown.Helpers;
using HandedDown.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace HandedDown.Services;

public class RecipeService : IRecipeService
{
    private readonly Container _container;

    // IConfiguration is injected rather than calling Environment.GetEnvironmentVariable directly
    // because Azure Functions automatically maps both local.settings.json (local dev) and Azure
    // App Settings (production) into IConfiguration — one approach works in both environments.
    public RecipeService(IConfiguration configuration)
    {
        // Fail fast on startup rather than silently failing on the first request — a missing
        // connection string will kill every call, so surfacing it immediately makes the root
        // cause obvious and avoids confusing runtime errors later.
        var connectionString = configuration["CosmosDbConnectionString"]
            ?? throw new InvalidOperationException("CosmosDbConnectionString is not configured.");

        var databaseName = configuration["CosmosDbDatabaseName"]
            ?? throw new InvalidOperationException("CosmosDbDatabaseName is not configured.");

        var containerName = configuration["CosmosDbContainerName"]
            ?? throw new InvalidOperationException("CosmosDbContainerName is not configured.");

        var client = new CosmosClient(connectionString, new CosmosClientOptions
        {
            // Custom serializer so [JsonProperty] attribute names are honored — without this,
            // the SDK writes PascalCase field names and Cosmos DB rejects or misreads documents.
            Serializer = new CosmosNewtonsoftSerializer()
        });

        // GetContainer creates a lightweight reference — it does not make a network call.
        // The actual connection is verified on the first database operation.
        _container = client.GetContainer(databaseName, containerName);
    }

    public async Task<IEnumerable<Recipe>> GetAllRecipesAsync()
    {
        try
        {
            var results = new List<Recipe>();
            var iterator = _container.GetItemQueryIterator<Recipe>(
                new QueryDefinition("SELECT * FROM c"));

            // Cosmos DB returns results in pages — a single ReadNextAsync call only retrieves
            // one page. The loop accumulates all pages before returning the complete list.
            while (iterator.HasMoreResults)
            {
                var page = await iterator.ReadNextAsync();
                results.AddRange(page);
            }

            return results;
        }
        catch (CosmosException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve recipes: {ex.Message}", ex);
        }
    }

    public async Task<Recipe?> GetRecipeByIdAsync(string id)
    {
        try
        {
            // ReadItemAsync targets a specific id + partition key directly, bypassing the query
            // engine entirely. This is the most efficient Cosmos DB operation (1 RU vs many for
            // a query) and should always be used when both the id and partition key are known.
            var response = await _container.ReadItemAsync<Recipe>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // Return null rather than throwing — not finding a recipe by id is an expected
            // outcome for a read. The Function layer decides whether to return 404 or handle
            // the absence differently.
            return null;
        }
        catch (CosmosException ex)
        {
            throw new InvalidOperationException($"Failed to retrieve recipe '{id}': {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Recipe>> GetRecipesByChapterAsync(string chapter)
    {
        try
        {
            var results = new List<Recipe>();

            // Parameterised query rather than string interpolation to prevent injection attacks.
            // The field name "chapter" matches the [JsonProperty("chapter")] attribute on the model —
            // Cosmos queries run against the stored JSON, not C# property names.
            var query = new QueryDefinition("SELECT * FROM c WHERE c.chapter = @chapter")
                .WithParameter("@chapter", chapter);

            var iterator = _container.GetItemQueryIterator<Recipe>(query);

            while (iterator.HasMoreResults)
            {
                var page = await iterator.ReadNextAsync();
                results.AddRange(page);
            }

            return results;
        }
        catch (CosmosException ex)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve recipes for chapter '{chapter}': {ex.Message}", ex);
        }
    }

    public async Task<Recipe> CreateRecipeAsync(Recipe recipe)
    {
        try
        {
            ValidateAndSanitize(recipe);

            // ID is always generated here, not by the caller — this prevents a client from
            // supplying an ID that could collide with or overwrite an existing document.
            recipe.Id = Guid.NewGuid().ToString();

            // CreatedDate is set at persistence time rather than relying on the model default,
            // which is set at object construction and could reflect when the request was
            // deserialized rather than when the record was actually saved.
            recipe.CreatedDate = DateTime.UtcNow;

            // Return response.Resource (the document as Cosmos stored it) rather than the input
            // recipe — this gives the caller the authoritative persisted state, including any
            // server-side defaults, rather than echoing back what was sent.
            var response = await _container.CreateItemAsync(recipe, new PartitionKey(recipe.Id));
            return response.Resource;
        }
        catch (CosmosException ex)
        {
            throw new InvalidOperationException($"Failed to create recipe: {ex.Message}", ex);
        }
    }

    public async Task<Recipe> UpdateRecipeAsync(string id, Recipe recipe)
    {
        try
        {
            // Always pin the id from the route parameter — if the request body contains a
            // different id, ignoring it prevents a client from accidentally or maliciously
            // updating a different document than the one targeted by the URL.
            recipe.Id = id;

            ValidateAndSanitize(recipe);

            // ReplaceItemAsync rather than UpsertItemAsync — Replace enforces that the document
            // must already exist, so a request to update a non-existent recipe fails explicitly
            // rather than silently creating a new one with an unexpected id.
            var response = await _container.ReplaceItemAsync(recipe, id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // Throw rather than return null — the caller explicitly targeted a resource that
            // doesn't exist, which is a client error, not a normal outcome.
            throw new KeyNotFoundException($"Recipe '{id}' not found.");
        }
        catch (CosmosException ex)
        {
            throw new InvalidOperationException($"Failed to update recipe '{id}': {ex.Message}", ex);
        }
    }

    public async Task DeleteRecipeAsync(string id)
    {
        try
        {
            await _container.DeleteItemAsync<Recipe>(id, new PartitionKey(id));
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException($"Recipe '{id}' not found.");
        }
        catch (CosmosException ex)
        {
            throw new InvalidOperationException($"Failed to delete recipe '{id}': {ex.Message}", ex);
        }
    }

    // Compiled once at class load — reusing the instance avoids repeated regex compilation on
    // every request, which is measurable overhead for a hot path like create/update.
    private static readonly Regex HtmlTagPattern =
        new(@"<[^>]*>", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

    // Strips HTML tags from every user-supplied string field before the recipe is persisted,
    // preventing stored XSS if the data is later rendered without escaping. Validation then
    // runs on the cleaned values so length limits are checked against what actually gets stored.
    private static void ValidateAndSanitize(Recipe recipe)
    {
        recipe.Title            = StripHtml(recipe.Title);
        recipe.Author           = StripHtml(recipe.Author);
        recipe.Chapter          = StripHtml(recipe.Chapter);
        recipe.ImageUrl         = StripHtml(recipe.ImageUrl);
        recipe.OriginalImageUrl = StripHtml(recipe.OriginalImageUrl);
        recipe.Ingredients      = recipe.Ingredients.Select(StripHtml).ToList();
        recipe.Instructions     = recipe.Instructions.Select(StripHtml).ToList();

        if (string.IsNullOrWhiteSpace(recipe.Title))
            throw new ArgumentException("Title is required.");
        if (recipe.Title.Length > 200)
            throw new ArgumentException("Title must be 200 characters or fewer.");

        if (recipe.Author.Length > 100)
            throw new ArgumentException("Author must be 100 characters or fewer.");

        if (recipe.Chapter.Length > 100)
            throw new ArgumentException("Chapter must be 100 characters or fewer.");

        if (recipe.Ingredients.Count == 0)
            throw new ArgumentException("At least one ingredient is required.");
        if (recipe.Ingredients.Count > 50)
            throw new ArgumentException("A recipe cannot have more than 50 ingredients.");
        if (recipe.Ingredients.Any(i => i.Length > 200))
            throw new ArgumentException("Each ingredient must be 200 characters or fewer.");

        if (recipe.Instructions.Count == 0)
            throw new ArgumentException("At least one instruction step is required.");
        if (recipe.Instructions.Count > 30)
            throw new ArgumentException("A recipe cannot have more than 30 instruction steps.");
        if (recipe.Instructions.Any(s => s.Length > 2000))
            throw new ArgumentException("Each instruction step must be 2000 characters or fewer.");

        ValidateUrl(recipe.ImageUrl,         nameof(recipe.ImageUrl));
        ValidateUrl(recipe.OriginalImageUrl, nameof(recipe.OriginalImageUrl));
    }

    private static void ValidateUrl(string url, string fieldName)
    {
        if (string.IsNullOrEmpty(url)) return;

        // javascript:// is blocked explicitly because a browser may execute it if the value is
        // ever placed in an href or src attribute, even if the host starts with https://.
        if (url.Contains("javascript://", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"{fieldName} contains a disallowed protocol.");

        if (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"{fieldName} must start with https://.");
    }

    private static string StripHtml(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return HtmlTagPattern.Replace(input, string.Empty);
    }
}
