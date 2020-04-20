using System.Text.Json.Serialization;

namespace Tayco.Web.Model
{
    public class Blog
    {
        [JsonIgnore]
        public string Name => $"{Title}: {Subtitle}";

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("subtitle")]
        public string Subtitle { get; set; }
    }
}
