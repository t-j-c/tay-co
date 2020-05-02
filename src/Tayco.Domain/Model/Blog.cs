using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tayco.Domain.Model
{
    public class Blog
    {
        [JsonIgnore]
        public string Name => string.IsNullOrWhiteSpace(Subtitle) ? Title : $"{Title}: {Subtitle}";

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("subtitle")]
        public string Subtitle { get; set; }

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("uploadDate")]
        [JsonConverter(typeof(BlogDateConverter))]
        public DateTime UploadDate { get; set; }

        [JsonIgnore]
        public Blog Previous { get; set; }
        
        [JsonIgnore]
        public Blog Next { get; set; }
    }

    public class BlogDateConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => 
            DateTime.ParseExact(reader.GetString(), "MMMM dd, yyyy", CultureInfo.InvariantCulture);

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
        }
    }
}
