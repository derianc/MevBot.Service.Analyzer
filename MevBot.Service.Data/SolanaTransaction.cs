using System.Text.Json.Serialization;

namespace MevBot.Service.Data
{
    public class SolanaTransaction
    {
        [JsonPropertyName("jsonrpc")]
        public string? jsonrpc { get; set; }

        [JsonPropertyName("method")]
        public string? method { get; set; }

        // Escape the reserved keyword and map it correctly.
        [JsonPropertyName("params")]
        public Params? @params { get; set; }
    }
}
