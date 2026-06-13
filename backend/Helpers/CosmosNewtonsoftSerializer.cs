using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace HandedDown.Helpers;

// The Cosmos SDK's built-in serializer uses System.Text.Json, which does not read [JsonProperty]
// attributes. Without this custom serializer, documents would be stored with PascalCase field names
// ("Id", "Title") instead of the lowercase names defined on the model ("id", "title"), breaking
// all queries and Cosmos DB's own requirement that the id field be lowercase.
public class CosmosNewtonsoftSerializer : CosmosSerializer
{
    // Static so a single JsonSerializer instance is shared across all calls — JsonSerializer is
    // thread-safe and expensive to construct due to contract resolver caching.
    private static readonly JsonSerializer _serializer = JsonSerializer.CreateDefault();

    public override T FromStream<T>(Stream stream)
    {
        using var sr = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(sr);
        return _serializer.Deserialize<T>(jsonReader)!;
    }

    public override Stream ToStream<T>(T input)
    {
        var ms = new MemoryStream();
        // leaveOpen: true — StreamWriter must not dispose the MemoryStream when it is disposed,
        // because the stream is returned to the Cosmos SDK for reading after this method returns.
        using (var sw = new StreamWriter(ms, leaveOpen: true))
        using (var jw = new JsonTextWriter(sw))
        {
            _serializer.Serialize(jw, input);
        }
        ms.Position = 0;
        return ms;
    }
}
