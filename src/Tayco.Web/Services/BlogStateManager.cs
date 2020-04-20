using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Tayco.Web.Model;

namespace Tayco.Web.Services
{
    public class BlogStateManager
    {
        private readonly HttpClient _client;
        private Blog[] _blogs;

        public BlogStateManager(HttpClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<Blog>> GetBlogsAsync()
        {
            await LoadBlogsAsync();
            return _blogs;
        }

        private async Task LoadBlogsAsync()
        {
            if (_blogs != null) return;

            var response = await _client.GetAsync("https://s3.us-east-2.amazonaws.com/blogs.tay-co.com/index.json");
            var stream = await response.Content.ReadAsStreamAsync();
            _blogs = await JsonSerializer.DeserializeAsync<Blog[]>(stream);
        }
    }
}
