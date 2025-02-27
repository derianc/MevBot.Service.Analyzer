using System.Text.Json.Serialization;

namespace MevBot.Service.Data
{
    public class Value
    {
        [JsonPropertyName("signature")]
        public string? signature { get; set; }

        [JsonPropertyName("err")]
        public object? err { get; set; }

        [JsonPropertyName("logs")]
        public List<string>? logs { get; set; }
    }
}
