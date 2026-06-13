using HandedDown.Models;
using HandedDown.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json;

namespace HandedDown.Functions;

public class RecipeFunctions
{
    private readonly IRecipeService _recipeService;

    // IRecipeService is injected by the DI container configured in Program.cs.
    // Functions never instantiate services directly — this keeps them testable and
    // ensures the singleton CosmosClient lifecycle is managed by the host, not here.
    public RecipeFunctions(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    [Function(nameof(GetAllRecipes))]
    public async Task<IActionResult> GetAllRecipes(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "recipes")] HttpRequest req)
    {
        try
        {
            var recipes = await _recipeService.GetAllRecipesAsync();
            return new OkObjectResult(recipes);
        }
        catch (Exception ex)
        {
            return ServerError($"Failed to retrieve recipes: {ex.Message}");
        }
    }

    // Route "recipes/chapter/{chapter}" must be declared before "recipes/{id}" so the ASP.NET
    // Core routing engine matches the literal segment "chapter" first and does not treat it as
    // an id value. Literal segments take precedence over parameter segments in route resolution.
    [Function(nameof(GetRecipesByChapter))]
    public async Task<IActionResult> GetRecipesByChapter(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "recipes/chapter/{chapter}")] HttpRequest req,
        string chapter)
    {
        try
        {
            var recipes = await _recipeService.GetRecipesByChapterAsync(chapter);
            return new OkObjectResult(recipes);
        }
        catch (Exception ex)
        {
            return ServerError($"Failed to retrieve recipes for chapter '{chapter}': {ex.Message}");
        }
    }

    [Function(nameof(GetRecipeById))]
    public async Task<IActionResult> GetRecipeById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "recipes/{id}")] HttpRequest req,
        string id)
    {
        try
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(id);

            // GetRecipeByIdAsync returns null rather than throwing when a recipe does not exist —
            // not found is a normal outcome for a read, so null is translated to 404 here in
            // the HTTP layer where status codes are decided.
            if (recipe is null)
                return new NotFoundResult();

            return new OkObjectResult(recipe);
        }
        catch (Exception ex)
        {
            return ServerError($"Failed to retrieve recipe '{id}': {ex.Message}");
        }
    }

    [Function(nameof(CreateRecipe))]
    public async Task<IActionResult> CreateRecipe(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "recipes")] HttpRequest req)
    {
        try
        {
            var recipe = await DeserializeBody<Recipe>(req);

            if (recipe is null)
                return new BadRequestObjectResult("Request body is missing or could not be parsed as a recipe.");

            var created = await _recipeService.CreateRecipeAsync(recipe);

            // 201 Created rather than 200 OK — signals that a new resource was created, which
            // lets clients and caches treat the response differently from a plain data fetch.
            return new ObjectResult(created) { StatusCode = StatusCodes.Status201Created };
        }
        catch (Exception ex)
        {
            return ServerError($"Failed to create recipe: {ex.Message}");
        }
    }

    [Function(nameof(UpdateRecipe))]
    public async Task<IActionResult> UpdateRecipe(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "recipes/{id}")] HttpRequest req,
        string id)
    {
        try
        {
            var recipe = await DeserializeBody<Recipe>(req);

            if (recipe is null)
                return new BadRequestObjectResult("Request body is missing or could not be parsed as a recipe.");

            var updated = await _recipeService.UpdateRecipeAsync(id, recipe);
            return new OkObjectResult(updated);
        }
        catch (KeyNotFoundException)
        {
            return new NotFoundResult();
        }
        catch (Exception ex)
        {
            return ServerError($"Failed to update recipe '{id}': {ex.Message}");
        }
    }

    [Function(nameof(DeleteRecipe))]
    public async Task<IActionResult> DeleteRecipe(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "recipes/{id}")] HttpRequest req,
        string id)
    {
        try
        {
            await _recipeService.DeleteRecipeAsync(id);

            // 204 No Content — the resource is gone, there is nothing to return.
            return new NoContentResult();
        }
        catch (KeyNotFoundException)
        {
            return new NotFoundResult();
        }
        catch (Exception ex)
        {
            return ServerError($"Failed to delete recipe '{id}': {ex.Message}");
        }
    }

    // Centralised so all functions deserialize request bodies the same way using Newtonsoft.Json,
    // which honours the [JsonProperty] attributes on models. Using System.Text.Json here would
    // silently ignore those attributes and produce null or empty model properties.
    private static async Task<T?> DeserializeBody<T>(HttpRequest req)
    {
        using var reader = new StreamReader(req.Body);
        var body = await reader.ReadToEndAsync();
        return JsonConvert.DeserializeObject<T>(body);
    }

    // Single place to build a 500 response so the format is consistent across all functions.
    private static ObjectResult ServerError(string message) =>
        new(message) { StatusCode = StatusCodes.Status500InternalServerError };
}
