using HandedDown.Models;

namespace HandedDown.Services;

// Defines the contract against which Function classes are written.
// Having an interface here means Functions depend on the abstraction, not the concrete Cosmos
// implementation — so individual functions can be unit tested by swapping in a mock service
// without needing a live database connection.
public interface IRecipeService
{
    Task<IEnumerable<Recipe>> GetAllRecipesAsync();
    Task<Recipe?> GetRecipeByIdAsync(string id);
    Task<IEnumerable<Recipe>> GetRecipesByChapterAsync(string chapter);
    Task<Recipe> CreateRecipeAsync(Recipe recipe);
    Task<Recipe> UpdateRecipeAsync(string id, Recipe recipe);
    Task DeleteRecipeAsync(string id);
}
