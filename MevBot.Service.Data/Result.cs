using System.Text.Json.Serialization;

namespace MevBot.Service.Data
{
    public class Result
    {
        [JsonPropertyName("context")]
        public Context? context { get; set; }

        [JsonPropertyName("value")]
        public Value? value { get; set; }
    }
}
