using Newtonsoft.Json;

namespace HandedDown.Models;

/// <summary>
/// Represents a family recipe stored in Cosmos DB.
/// The Id maps to the Cosmos DB partition key.
/// </summary>
public class Recipe
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Free-text ingredient list as interpreted from the original handwritten image.
    /// </summary>
    [JsonProperty("ingredients")]
    public string Ingredients { get; set; } = string.Empty;

    [JsonProperty("instructions")]
    public string Instructions { get; set; } = string.Empty;

    /// <summary>
    /// The cookbook chapter this recipe belongs to (e.g. "Desserts", "Mains").
    /// </summary>
    [JsonProperty("chapter")]
    public string Chapter { get; set; } = string.Empty;

    /// <summary>
    /// The family member who originally wrote the recipe.
    /// </summary>
    [JsonProperty("author")]
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// URL of the processed/display image in Blob Storage.
    /// </summary>
    [JsonProperty("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL of the raw uploaded handwritten image, kept for re-processing if needed.
    /// </summary>
    [JsonProperty("originalImageUrl")]
    public string OriginalImageUrl { get; set; } = string.Empty;

    [JsonProperty("createdDate")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
