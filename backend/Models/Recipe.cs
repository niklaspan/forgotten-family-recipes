// Newtonsoft.Json is used instead of System.Text.Json because the Cosmos DB SDK's default
// serializer doesn't read [JsonProperty] attributes — so without a custom Newtonsoft serializer
// (CosmosNewtonsoftSerializer), property names in documents would be PascalCase ("Id", "Title")
// instead of the lowercase names Cosmos DB expects ("id", "title").
using Newtonsoft.Json;

namespace HandedDown.Models;

/// <summary>
/// Represents a family recipe stored in Cosmos DB.
/// Id is used as both the document id and the partition key — this keeps lookups cheap
/// (single-partition reads) at the cost of cross-partition queries for chapter filtering,
/// which is acceptable given the expected data volume.
/// </summary>
public class Recipe
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Free-text ingredient list as interpreted from the original handwritten image.
    /// Stored as a single string rather than a structured list because handwritten recipes
    /// rarely follow a consistent format — Claude extracts what it can, as-is.
    /// </summary>
    [JsonProperty("ingredients")]
    public string Ingredients { get; set; } = string.Empty;

    [JsonProperty("instructions")]
    public string Instructions { get; set; } = string.Empty;

    /// <summary>
    /// The cookbook chapter this recipe belongs to (e.g. "Desserts", "Mains").
    /// Used to group recipes in the UI — filtering by chapter is the primary browse pattern.
    /// </summary>
    [JsonProperty("chapter")]
    public string Chapter { get; set; } = string.Empty;

    /// <summary>
    /// The family member who originally wrote the recipe.
    /// Preserved to maintain the personal character of the cookbook — knowing who wrote a
    /// recipe is part of what makes it meaningful.
    /// </summary>
    [JsonProperty("author")]
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// URL of the processed/display image in Blob Storage.
    /// </summary>
    [JsonProperty("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL of the raw uploaded handwritten image.
    /// Kept separately so the original can be re-sent to Claude if the AI interpretation
    /// needs to be improved later without the user having to re-upload.
    /// </summary>
    [JsonProperty("originalImageUrl")]
    public string OriginalImageUrl { get; set; } = string.Empty;

    // UTC is used throughout to avoid ambiguity when the app is used across time zones
    // or when the Azure Function runs in a region different from the user's location.
    [JsonProperty("createdDate")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
