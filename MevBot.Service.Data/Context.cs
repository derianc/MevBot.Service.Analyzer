using System.Text.Json.Serialization;

namespace MevBot.Service.Data
{
    public class Context
    {
        [JsonPropertyName("slot")]
        public long slot { get; set; }
    }
}
