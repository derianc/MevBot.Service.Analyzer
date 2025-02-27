using System.Text.Json.Serialization;

namespace MevBot.Service.Data
{
    public class Params
    {
        [JsonPropertyName("result")]
        public Result? result { get; set; }

        [JsonPropertyName("subscription")]
        public int subscription { get; set; }
    }
}
