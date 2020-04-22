using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
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

        [JsonPropertyName("uploadDate")]
        [JsonConverter(typeof(BlogDateConverter))]
        public DateTime UploadDate { get; set; }
    }

    public class BlogDateConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var json = reader.GetString();
            Debug.WriteLine(json);
            return DateTime.ParseExact(json, "MMMM dd, yyyy", CultureInfo.InvariantCulture);            
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
        }
    }
}
