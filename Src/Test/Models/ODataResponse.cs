using System.Collections.Generic;
using Newtonsoft.Json;

namespace Test.Models;

public class ODataResponse<T>
{
    [JsonProperty("@odata.context")]
    public string Context { get; set; } = string.Empty;

    [JsonProperty("value")]
    public T[] Value { get; set; } = Array.Empty<T>();

    [JsonProperty("@odata.count")]
    public int? Count { get; set; }
}
