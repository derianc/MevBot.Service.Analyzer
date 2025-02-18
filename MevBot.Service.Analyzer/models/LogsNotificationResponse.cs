using System.Text.Json.Serialization;

namespace MevBot.Service.Analyzer.models
{
    public class LogsNotificationResponse
    {
        [JsonPropertyName("jsonrpc")]
        public string? jsonrpc { get; set; }

        [JsonPropertyName("method")]
        public string? method { get; set; }

        // Escape the reserved keyword and map it correctly.
        [JsonPropertyName("params")]
        public Params? @params { get; set; }
    }

    public class Params
    {
        [JsonPropertyName("result")]
        public Result? result { get; set; }

        [JsonPropertyName("subscription")]
        public int subscription { get; set; }
    }

    public class Result
    {
        [JsonPropertyName("context")]
        public Context? context { get; set; }

        [JsonPropertyName("value")]
        public Value? value { get; set; }
    }

    public class Context
    {
        [JsonPropertyName("slot")]
        public long slot { get; set; }
    }

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
